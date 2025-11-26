#!/bin/bash
set -e

echo "===================================="
echo "Claude Code Dev Environment Setup"
echo "===================================="

# Only run in Claude Code remote environments
if [ -z "$CLAUDE_CODE_REMOTE" ]; then
  echo "Running locally - skipping setup"
  exit 0
fi

echo "Claude Code remote detected"
echo ""

# Check if dotnet is installed
echo "===================================="
echo "Checking .NET SDK"
echo "===================================="
if ! command -v dotnet &> /dev/null; then
  echo "✗ .NET SDK not found"
  echo "Please install .NET 10 SDK manually"
  echo "Visit: https://dotnet.microsoft.com/download/dotnet/10.0"
  exit 1
fi

CURRENT_VERSION=$(dotnet --version 2>/dev/null || echo "0.0.0")
MAJOR_VERSION=$(echo "$CURRENT_VERSION" | cut -d. -f1)

if [ "$MAJOR_VERSION" -ge 10 ]; then
  echo "✓ .NET $CURRENT_VERSION installed"
else
  echo "⚠ .NET $CURRENT_VERSION found, but version 10+ required"
  echo "Visit: https://dotnet.microsoft.com/download/dotnet/10.0"
fi
echo ""

# Restore .NET local tools
echo "===================================="
echo "Restoring .NET Local Tools"
echo "===================================="
if [ ! -f ".config/dotnet-tools.json" ]; then
  echo "⚠ No .config/dotnet-tools.json found - skipping"
elif dotnet tool restore; then
  echo "✓ .NET tools restored"
else
  echo "✗ Failed to restore .NET tools"
  exit 1
fi
echo ""

# Install Husky git hooks
echo "===================================="
echo "Installing Husky Git Hooks"
echo "===================================="
if dotnet tool list | grep -q "husky"; then
  if dotnet husky install; then
    echo "✓ Husky git hooks installed"
  else
    echo "⚠ Husky installation completed with warnings"
  fi
else
  echo "⚠ Husky tool not found"
fi
echo ""

echo "===================================="
echo "Setup Complete"
echo "===================================="
echo "✓ Development environment ready"
echo ""

exit 0
