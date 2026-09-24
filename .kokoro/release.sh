#!/bin/bash
set -euo pipefail

# Default to dry-run for safety unless DRY_RUN=false is explicitly passed.
DRY_RUN="${DRY_RUN:-true}"
if [[ "${DRY_RUN}" != "true" && "${DRY_RUN}" != "false" ]]; then
  echo "ERROR: DRY_RUN must be 'true' or 'false' (got '${DRY_RUN}')." >&2
  exit 1
fi

# -----------------------------------------------------------------------------
# 1. Workspace & Directory Resolution
# -----------------------------------------------------------------------------
REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${REPO_DIR}"

echo "=== Building and Releasing from: ${REPO_DIR} (DRY_RUN=${DRY_RUN}) ==="

# -----------------------------------------------------------------------------
# 2. Environment & Tooling Setup
# -----------------------------------------------------------------------------
echo "=== Environment Info ==="
dotnet --info

# -----------------------------------------------------------------------------
# 3. Clean, Test, and Build Distribution Packages
# -----------------------------------------------------------------------------
echo "=== Restoring tools and dependencies ==="
dotnet tool restore
dotnet restore

echo "=== Running tests ==="
dotnet test -c Release

echo "=== Building and packing release package ==="
rm -rf artifacts/
dotnet pack Google.Ads.DataManager.Util/src/Google.Ads.DataManager.Util.csproj \
  -c Release \
  -o artifacts/

# -----------------------------------------------------------------------------
# 4. Upload to Internal Exit Gate Artifact Registry
# -----------------------------------------------------------------------------
EXIT_GATE_PROJECT="oss-exit-gate-prod"
EXIT_GATE_LOCATION="us"
EXIT_GATE_REPOSITORY="measurement-devrel--nuget"
# Package name in Artifact Registry and Exit Gate project.txtpb must be lowercase.
# Display casing on nuget.org is preserved automatically from the .nupkg metadata.
PACKAGE_NAME="google.ads.datamanager.util"

# Extract the package version from the .csproj file (required by 'gcloud
# artifacts generic upload').
PACKAGE_VERSION=$(sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p' Google.Ads.DataManager.Util/src/Google.Ads.DataManager.Util.csproj | tr -d '[:space:]')
if [[ -z "${PACKAGE_VERSION}" ]]; then
  echo "Error: Could not extract PACKAGE_VERSION from Google.Ads.DataManager.Util/src/Google.Ads.DataManager.Util.csproj" >&2
  exit 1
fi
echo "Package: ${PACKAGE_NAME}, Version: ${PACKAGE_VERSION}"

# Delete existing package from the Exit Gate staging repository if present.
# This prevents generic upload failures on retries or re-releases of the same version.
if gcloud artifacts packages describe "${PACKAGE_NAME}" \
    --project="${EXIT_GATE_PROJECT}" \
    --location="${EXIT_GATE_LOCATION}" \
    --repository="${EXIT_GATE_REPOSITORY}" &>/dev/null; then
  echo "=== Deleting existing package '${PACKAGE_NAME}' from Exit Gate staging repository ==="
  gcloud artifacts packages delete "${PACKAGE_NAME}" \
    --project="${EXIT_GATE_PROJECT}" \
    --location="${EXIT_GATE_LOCATION}" \
    --repository="${EXIT_GATE_REPOSITORY}" \
    --quiet
else
  echo "=== Skipping deletion. Package '${PACKAGE_NAME}' not found in staging repository. 🙂 ==="
fi

echo "=== Uploading package artifacts to Exit Gate staging repository ==="
gcloud artifacts generic upload \
  --project="${EXIT_GATE_PROJECT}" \
  --location="${EXIT_GATE_LOCATION}" \
  --repository="${EXIT_GATE_REPOSITORY}" \
  --package="${PACKAGE_NAME}" \
  --version="${PACKAGE_VERSION}" \
  --source-directory=artifacts/

# -----------------------------------------------------------------------------
# 5. Trigger Exit Gate Release via GCS Manifest
# -----------------------------------------------------------------------------
# If DRY_RUN is set to "true", stop here so you can verify AR staging
# without publishing to public NuGet.
if [[ "${DRY_RUN}" == "true" ]]; then
  echo "=== DRY_RUN is enabled. Skipping manifest upload to Exit Gate. ==="
  echo "Artifacts are staged in Artifact Registry."
  exit 0
fi

echo "=== Creating targeted release manifest for ${PACKAGE_NAME} ==="
cat <<EOF > manifest.json
{
  "publish_all": false,
  "publishing_groups": [
    {
      "packages": [
        {
          "name": "${PACKAGE_NAME}",
          "version": "${PACKAGE_VERSION}"
        }
      ]
    }
  ]
}
EOF

EXIT_GATE_BUCKET="gs://oss-exit-gate-prod-projects-bucket/measurement-devrel/nuget/manifests"
MANIFEST_NAME="manifest-$(date +%Y%m%d%H%M%S).json"

echo "=== Uploading manifest to ${EXIT_GATE_BUCKET}/${MANIFEST_NAME} ==="
gcloud storage cp manifest.json "${EXIT_GATE_BUCKET}/${MANIFEST_NAME}"

echo "========================================================================="
echo "Release successfully triggered! Exit Gate will now verify BCID and publish to NuGet."
echo "========================================================================="
