#!/bin/bash
set -e

# Configuration
MARKETPLACE_REPO="L-Sypniewski/claude-code-toolkit"
MARKETPLACE_BRANCH="master"
MARKETPLACE_URL="https://github.com/${MARKETPLACE_REPO}.git"

# Only run in web environments
if [ "$CLAUDE_CODE_REMOTE" != "true" ]; then
  echo "Running locally - plugins already installed"
  exit 0
fi

echo "===================================="
echo "Claude Code Plugin Auto-Installer"
echo "===================================="
echo "Marketplace: ${MARKETPLACE_REPO}"
echo ""

# Install OpenSpec globally
echo "===================================="
echo "Installing OpenSpec"
echo "===================================="
npm install -g @fission-ai/openspec@latest 2>&1 | tail -3
echo "✓ OpenSpec installed"
echo ""

# Clone marketplace repository to temporary location
TEMP_DIR=$(mktemp -d)
trap 'rm -rf "$TEMP_DIR"' EXIT

echo "===================================="
echo "Cloning Plugin Marketplace"
echo "===================================="
if ! git clone --depth 1 --branch "$MARKETPLACE_BRANCH" "$MARKETPLACE_URL" "$TEMP_DIR" 2>&1 | tail -3; then
  echo "✗ Failed to clone marketplace repository"
  exit 1
fi
echo "✓ Marketplace cloned"
echo ""

# Create .claude directories in project root
# Use CLAUDE_PROJECT_DIR if set, otherwise find git root
if [ -n "$CLAUDE_PROJECT_DIR" ]; then
  PROJECT_ROOT="$CLAUDE_PROJECT_DIR"
else
  PROJECT_ROOT=$(git rev-parse --show-toplevel 2>/dev/null || pwd)
fi

CLAUDE_DIR="$PROJECT_ROOT/.claude"
mkdir -p "$CLAUDE_DIR/agents" "$CLAUDE_DIR/commands"

# Install plugins by copying files
echo "===================================="
echo "Installing Plugins"
echo "===================================="

AGENT_COUNT=0
COMMAND_COUNT=0

# Copy all agents
if [ -d "$TEMP_DIR/plugins" ]; then
  for plugin_dir in "$TEMP_DIR/plugins"/*; do
    if [ -d "$plugin_dir/agents" ]; then
      plugin_name=$(basename "$plugin_dir")
      echo "Installing agents from: $plugin_name"

      for agent in "$plugin_dir/agents"/*.md; do
        if [ -f "$agent" ]; then
          agent_name=$(basename "$agent")
          cp "$agent" "$CLAUDE_DIR/agents/"
          echo "  ✓ $agent_name"
          AGENT_COUNT=$((AGENT_COUNT + 1))
        fi
      done
    fi

    if [ -d "$plugin_dir/commands" ]; then
      plugin_name=$(basename "$plugin_dir")
      echo "Installing commands from: $plugin_name"

      for command in "$plugin_dir/commands"/*.md; do
        if [ -f "$command" ]; then
          command_name=$(basename "$command")
          cp "$command" "$CLAUDE_DIR/commands/"
          echo "  ✓ $command_name"
          COMMAND_COUNT=$((COMMAND_COUNT + 1))
        fi
      done
    fi
  done
fi

echo ""
echo "===================================="
echo "Installation Summary"
echo "===================================="
echo "Agents installed: ${AGENT_COUNT}"
echo "Commands installed: ${COMMAND_COUNT}"
echo ""

if [ $AGENT_COUNT -gt 0 ] || [ $COMMAND_COUNT -gt 0 ]; then
  echo "✓ All plugins installed successfully!"

  echo ""
  echo "Installed Agents:"
  ls -1 "$CLAUDE_DIR/agents/" 2>/dev/null | sed 's/^/  - /' || echo "  (none)"

  echo ""
  echo "Installed Commands:"
  ls -1 "$CLAUDE_DIR/commands/" 2>/dev/null | sed 's/^/  - /' || echo "  (none)"
else
  echo "⚠ No plugins found to install"
fi

exit 0
