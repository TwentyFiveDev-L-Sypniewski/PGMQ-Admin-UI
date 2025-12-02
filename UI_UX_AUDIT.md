# UI/UX Audit - PGMQ Admin UI

Generated: 2025-12-02

## Overview
This document contains a comprehensive UI/UX audit of the PGMQ Admin UI application, evaluating each page across mobile (428×926) and desktop (1160×720) viewports.

---

# Page – Home / Queues

## Screen size – 428×926
**Issues**
- Card-based layout creates excessive vertical scrolling with only three queues visible, requiring significant thumb travel to access lower items
- Metric labels (Total, In-Flight, Archived) use insufficient typographic differentiation from their values, creating weak information hierarchy within cards
- Action buttons (View Details, Delete) lack visual weight distinction, making the destructive Delete action appear equal in importance to the primary navigation action
- Side navigation drawer remains visible as a collapsed hamburger icon without clear affordance indicating it can be expanded
- Connection status indicator (Online) in top-right corner has minimal contrast against blue header background, reducing scanability
- Footer contains two text elements (Fluent UI Blazor link, PGMQ Admin UI label) with unclear relationship and purpose, creating visual clutter
- Queue name typography lacks emphasis, making it difficult to quickly scan and differentiate between queues in the card stack
- White space distribution within cards feels unbalanced, with metrics clustered tightly while action buttons have generous spacing
- No empty state guidance or visual cues for when queue lists are empty

**Improvements**
- Implement higher contrast typographic scale: increase queue name font size and weight to establish clear primary-secondary-tertiary hierarchy
- Redesign action buttons with visual priority: use contained button style for View Details, text-only or outlined style for Delete to signal danger through color alone
- Add subtle visual separator between metric groups within cards to improve scannability
- Increase header contrast for status indicator: use badge or pill component with higher contrast background
- Consolidate footer into single centered element or remove secondary branding if redundant
- Consider list view option as alternative to cards for improved information density on mobile
- Add touch target spacing between cards (minimum 8px) to prevent accidental taps
- Implement pull-to-refresh gesture affordance for queue list updates
- Design empty state illustration with clear call-to-action when no queues exist

## Screen size – 1160×720
**Issues**
- Table layout exhibits poor space utilization, with excessive horizontal whitespace in center columns and compressed action buttons on the right
- Navigation sidebar width (approximately 140px) creates awkward aspect ratio with main content area, leaving underutilized space in left margin
- Column headers lack sufficient visual weight to establish clear distinction from table data rows
- Action links (View Details, Delete) appear as inline text without button affordances, reducing perceived clickability
- Numerical columns (Total, In-Flight, Archived) display centered alignment, which reduces scannability for comparing values vertically
- Page title "Queues" and "Create Queue" button occupy substantial vertical real estate without providing contextual information or metrics summary
- Table rows lack hover states or zebra striping, making row tracking difficult across wide viewport
- Footer anchors to bottom of viewport rather than content, creating disconnected floating appearance on pages with few items
- No visual indication of sortable columns beyond subtle cursor change on column headers
- Missing pagination controls or item count when table has limited rows, providing no context about data completeness

**Improvements**
- Implement responsive table column widths: allocate proportional space based on content importance (Queue Name: 30%, metrics: 15% each, Actions: 25%)
- Add subtle background tint or border to table rows on hover to improve tracking and indicate interactivity
- Right-align numerical columns for improved vertical scanning and mental calculation
- Redesign action column: use icon buttons with tooltips instead of text links to reduce horizontal width requirements
- Add visual weight to column headers: increase font weight, add subtle background color, or implement divider lines
- Condense header area: align "Queues" title and "Create Queue" button on same horizontal line, add queue count summary beside title
- Implement zebra striping with subtle alternating row backgrounds (5% opacity) to improve readability
- Position footer to stick to content bottom rather than viewport bottom
- Add explicit sort indicators (arrows) to sortable column headers with active state styling
- Include summary metrics panel above table showing aggregate counts across all queues
- Consider collapsible sidebar navigation to reclaim horizontal space for content area

---

# Page – Queue Detail: Archived Tab

## Screen size – 428×926
**Issues**
- Card-based message layout creates excessive vertical scrolling with large touch targets consuming significant screen real estate per message
- Message ID labels use inconsistent typographic treatment, appearing as hyperlinks (blue text) without clear indication of their interactivity or navigation purpose
- JSON message content displays in raw format without syntax highlighting or formatting, reducing readability and requiring horizontal scrolling for longer values
- Metadata fields (Created, Read Count) use weak visual hierarchy with label and value appearing in similar weight and size, making quick scanning difficult
- Tab navigation (Messages, Archived, Metrics) lacks visual indication of overflow, with "+1" indicator positioned awkwardly and cryptic in meaning
- Page header consumes excessive vertical space (approximately 240px) with action buttons, navigation, and tabs stacked vertically, reducing content viewport
- Pagination controls at bottom lack sufficient touch target spacing and appear visually disconnected from the message cards above
- Navigation drawer remains expanded on mobile, occupying approximately 25% of screen width and reducing content area unnecessarily
- Refresh button positioned in top-right of content area lacks prominent placement for a critical refresh action
- No indication of message archive timestamp versus original creation timestamp, creating ambiguity in temporal information
- Card shadows and borders create visual noise without improving content grouping or hierarchy
- Message content truncation or preview is absent, forcing full display of potentially lengthy JSON payloads in list view

**Improvements**
- Implement collapsible navigation drawer on mobile with overlay pattern to reclaim horizontal space for content
- Redesign message cards with stronger typographic hierarchy: increase message ID size and weight, reduce metadata label opacity to 60-70%
- Add JSON syntax highlighting with collapsible/expandable affordance for message content to improve readability and reduce initial vertical height
- Consolidate page header: move Send Message and Back to Queues buttons into navigation bar or bottom sheet to reduce vertical stack height
- Redesign tab overflow indicator with clearer affordance (e.g., "Metrics →" or horizontal scroll hint) instead of cryptic "+1"
- Implement denser card layout: reduce internal padding from ~24px to ~16px, use single-line metadata display with inline labels
- Add pull-to-refresh gesture as primary refresh mechanism, relocate refresh button to header toolbar for secondary access
- Implement virtual scrolling or pagination with "Load More" button to improve performance with large archived message sets
- Add visual distinction between archived timestamp and original creation timestamp (e.g., "Archived: 2025-12-02 19:03" vs "Created: 2025-12-02 17:29")
- Reduce card elevation from multiple shadows to single subtle shadow to minimize visual noise
- Add message preview/truncation with "Expand" affordance: show first 50 characters with "..." and tap-to-expand behavior
- Include empty state design for when no archived messages exist, with clear explanation and optional action to view active messages

## Screen size – 1160×720
**Issues**
- Table layout uses excessive whitespace between columns, particularly in Message column which could accommodate more content width
- Column header typographic weight insufficient to distinguish from table data, reducing scanability of row content
- Message content displays in raw JSON format without syntax highlighting, making complex payloads difficult to parse visually
- Timestamp format inconsistent with mobile view, showing full ISO format (2025-12-02 19:03:35) versus abbreviated version (2025-12-02 19:03)
- Table lacks hover states on rows, reducing affordance for potential row-level interactions
- No indication of whether table columns are sortable, despite Message ID and Created appearing to be logical sort candidates
- Pagination controls use icon-only buttons without text labels, reducing clarity of navigation options for users unfamiliar with icon conventions
- Action buttons area (Send Message, Back to Queues) creates visual imbalance with unequal button weights and inconsistent spacing
- Refresh button positioned asymmetrically in content area header, appearing disconnected from table controls
- No visible actions column for archived messages, suggesting potential feature parity gap with active messages tab
- Tab navigation lacks active state underline or background treatment beyond bold text weight
- Vertical spacing between page header and table content feels compressed (approximately 16px), reducing visual breathing room
- Message column content lacks truncation or modal expansion, potentially causing layout issues with extremely long JSON payloads
- Footer remains fixed to viewport bottom with significant empty space above, creating disconnected appearance

**Improvements**
- Implement proportional column widths: Message ID (15%), Message (50%), Created (20%), Read Count (15%) to optimize content display
- Add hover state to table rows with subtle background tint (5% opacity) to indicate interactivity and improve tracking
- Integrate JSON syntax highlighting in Message column with tooltip or modal expansion for full content preview
- Standardize timestamp format across viewports: use abbreviated format (YYYY-MM-DD HH:mm) for consistency and space efficiency
- Add explicit sort indicators to sortable column headers (Message ID, Created, Read Count) with visual active state for current sort
- Enhance pagination controls with text labels alongside icons ("Previous", "Next") or tooltip affordances for clarity
- Redesign action button group with consistent styling: use same button variant (both contained or both outlined) and equal spacing
- Reposition Refresh button into table header toolbar area, aligned with column headers for logical grouping
- Increase typographic weight of column headers: use semibold weight (600) versus regular (400) for data rows
- Add subtle zebra striping to table rows with alternating background tint (3% opacity) to improve row tracking across wide viewport
- Enhance tab active state with bottom border accent (3px) and background tint to strengthen visual indication
- Increase vertical spacing between header and table to 32px for improved visual separation and content hierarchy
- Implement message content truncation with "View Full Message" modal or expandable row pattern for lengthy JSON payloads
- Add context-sensitive actions for archived messages (e.g., "Restore to Queue", "Delete Permanently") with appropriate visual treatment
- Position footer to stick to content bottom rather than viewport, or integrate pagination into footer area to reduce visual disconnection
- Include data density controls (compact/comfortable/spacious) to allow user preference for information density

---

# Page – Queue Detail: Messages Tab

## Screen size – 428×926
**Issues**
- Table layout on narrow viewport creates severe horizontal scrolling—6 columns with substantial data compress into 428px width, forcing users to pan extensively to access action buttons
- Column headers (Message ID, Visibility Timeout, Created, Read Count, Actions) lack truncation or abbreviation strategy for constrained width
- Timestamp values (12/2/2025 7:30:5..., 2025-12-02 17:4...) are prematurely truncated mid-character with ellipses, breaking readability and preventing users from understanding temporal context
- Action icons (trash and archive) positioned at extreme right edge require horizontal scroll to discover and interact with—poor affordance for critical destructive operations
- Message content column displays raw JSON ({"a": "b"}) without formatting, expansion controls, or preview optimization for small screens
- Visual density is inconsistent—generous whitespace surrounds table while internal cell padding appears cramped, creating imbalanced rhythm
- No visual indication that table is horizontally scrollable—lacks scroll shadows, gradient hints, or pagination alternatives for mobile-first interaction
- Tab bar (Messages, Archived, Metrics) uses full-width text labels without considering condensed representation for narrow viewports
- Pagination controls at bottom retain desktop sizing and spacing, consuming disproportionate vertical real estate relative to content density
- "3 items" counter and multi-button pagination (first/prev/next/last) lack prioritization—all controls receive equal visual weight despite varying utility

**Improvements**
- Implement responsive card-based layout for mobile viewports—display each message as a discrete card containing ID, timestamp, read count, and actions in a vertically stacked arrangement
- Transform tabular data into scannable information hierarchy within cards: prominent ID header, secondary metadata (creation date, read count), and bottom-aligned action buttons
- Provide message content expansion through progressive disclosure—show preview or character count with tap-to-expand modal or accordion pattern
- Redesign timestamp presentation for mobile legibility—use relative time format (e.g., "2 hours ago") or shortened date notation (Dec 2, 17:40) to eliminate truncation
- Position action buttons within immediate visual proximity to message ID—eliminate need for horizontal scrolling to access critical operations
- Introduce vertical list layout with clear card boundaries, adequate inter-card spacing (minimum 16px), and tap target sizing meeting 44×44px minimum for touch interfaces
- Replace multi-button pagination with simplified previous/next navigation or infinite scroll pattern optimized for touch-based browsing
- Consider collapsible or icon-based tab navigation for narrow viewports to reclaim horizontal space
- Add visual cues for scrollable content if table layout is retained—subtle gradient overlay at overflow edges or persistent scroll indicators
- Establish mobile-specific typographic scale—reduce header size, optimize line height for single-column reading, and ensure minimum 16px body text to prevent zoom on iOS

## Screen size – 1160×720
**Issues**
- Desktop viewport unexpectedly displays card-based layout instead of optimized tabular grid—fails to leverage available horizontal space for data density and scanability
- Card layout creates excessive vertical scrolling—three messages with generous card padding consume full viewport height, hiding pagination controls below fold
- Each message card allocates equal visual weight to all metadata fields—no hierarchy distinguishes critical information (ID, created date) from supplementary details (visibility timeout, read count)
- Visibility Timeout displays raw ISO 8601 timestamp (12/2/2025 7:31:19 PM +00:00) without contextual formatting—users must mentally parse timezone-aware datetime instead of receiving human-readable "expires in X minutes" representation
- Action buttons (Delete, Archive) within cards use mixed iconography and labels—inconsistent with column-based action patterns established elsewhere in application
- Large whitespace gaps between cards and within card internal layout create loose spatial rhythm—undermines perception of data density appropriate for admin dashboard
- Message content displays raw JSON without syntax highlighting, collapsible structure, or copy-to-clipboard affordance—reduces utility for developers troubleshooting message payloads
- Horizontal space utilization is suboptimal—cards constrained to narrow center column leave significant margin whitespace unused at 1160px viewport
- Tab navigation (Messages/Archived/Metrics with +1 overflow indicator) positions tabs below action buttons, inverting expected hierarchical arrangement where navigation precedes content
- "Refresh" button positioned at equivalent visual level to section heading "Messages"—lacks sufficient emphasis or distinction as interactive control
- Pagination controls mirror mobile implementation despite desktop affording space for expanded options (items per page selector, direct page input)

**Improvements**
- Implement full-width data grid layout for desktop viewports—leverage horizontal space to display all message attributes in scannable columnar format
- Establish clear visual hierarchy through column sizing: prioritize Message ID (fixed narrow), Message content (flexible wide), timestamps (medium fixed), counts (narrow fixed), actions (fixed right-aligned)
- Apply zebra striping or subtle row hover states to enhance row tracking across wide horizontal spans
- Transform Visibility Timeout presentation to relative countdown format (e.g., "Expires in 4m 32s" or "28 minutes remaining") with tooltip showing absolute timestamp on hover
- Standardize action column to icon-only buttons with consistent sizing, spacing, and tooltip labels—align with patterns from queue list table
- Optimize message content column for JSON data—introduce syntax highlighting, collapsible nested structures, character limit with expand control, and one-click copy functionality
- Reduce inter-row vertical spacing to 8-12px to increase visible message density and minimize scrolling required to view full dataset
- Reposition tab navigation above page heading to establish proper information architecture—navigation context precedes content area
- Differentiate "Refresh" button through position (top-right alignment near tabs) and visual treatment (ghost or outline style) to distinguish from primary actions
- Enhance pagination with desktop-appropriate controls—add items-per-page selector (10/25/50/100), display range indicator ("Showing 1-3 of 3"), and consider jump-to-page input for larger datasets
- Introduce column sorting interactions—enable users to reorder messages by ID, creation timestamp, or read count through clickable column headers with directional indicators
- Establish maximum content width constraint (1200-1400px) with centered alignment to prevent extreme horizontal stretching on ultra-wide displays while maintaining optimal line length

---

# Page – Health

## Screen size – 428×926
**Issues**
- Page displays only a single text string "Healthy" in the top-left corner with no additional context, visual treatment, or layout structure
- Text appears as standard body copy without any semantic emphasis, status indicator styling, or iconography to reinforce meaning
- Complete absence of visual hierarchy leaves the page feeling incomplete and unfinished
- No container, card, or bounding element frames the status message, creating floating text with no spatial relationship to viewport edges
- Missing branding, navigation, or page structure elements that establish consistency with the rest of the application
- Text positioning lacks intentional alignment - appears to follow default browser margin behavior rather than designed placement
- Color treatment provides no semantic reinforcement of health status (no green success indicator, badge, or icon)
- Excessive negative space creates impression of broken page or failed content load rather than intentional minimal design
- No timestamp, metadata, or contextual information about what "Healthy" refers to or when status was last checked
- Missing actionable elements such as refresh button, back navigation, or links to related system information
- Typography scale does not differentiate this status message from standard body text, reducing scannability
- Page lacks any responsive adaptation - same minimal presentation regardless of available screen real estate

**Improvements**
- Center status message both horizontally and vertically within viewport to create intentional, balanced composition
- Implement semantic status card component with subtle background tint, border, and adequate padding (minimum 24px) to create visual containment
- Add success-state icon (checkmark, shield, or heartbeat symbol) adjacent to text to provide visual reinforcement of healthy status
- Apply color semantics: use green accent color for border or icon to immediately communicate positive system state
- Increase typographic weight and scale: use heading level typography (H3 or H4) with medium or semibold weight to establish message importance
- Include timestamp with last-checked information below status message in smaller, secondary typography
- Add contextual label above status message (e.g., "System Status" or "PGMQ Health") to provide semantic context
- Implement status details panel below primary message showing specific health metrics (database connection, queue accessibility, response time)
- Include "Refresh Status" button below status card to provide user control over status updates
- Add breadcrumb navigation or back link to maintain connection with application navigation structure
- Consider animated icon or subtle pulse effect on status indicator to suggest live monitoring
- Implement alternative error state design with red accent colors and warning iconography for unhealthy status display

## Screen size – 1160×720
**Issues**
- Identical presentation to mobile viewport demonstrates complete absence of responsive design adaptation
- Single text string "Healthy" appears in top-left corner without leveraging available horizontal or vertical space
- Missing opportunity to display detailed health dashboard with multiple status indicators, metrics panels, or system information cards
- No navigation chrome, header, sidebar, or structural elements connect page to broader application experience
- Text positioning suggests browser default margins rather than intentional desktop-optimized layout
- Excessive whitespace across entire viewport creates impression of incomplete page render or content loading failure
- Typography remains body-text scale despite ample space for larger, more prominent status display
- Absence of data visualization, charts, or expanded metrics despite desktop context typically supporting richer information displays
- No multi-column layout, grid structure, or card-based organization to present comprehensive health information
- Missing actionable dashboard elements such as refresh controls, time range selectors, or drill-down navigation
- Color and contrast treatment identical to mobile, missing opportunity for more sophisticated desktop-appropriate visual design
- Footer, header, and standard application chrome completely absent, breaking consistency with other pages in application

**Improvements**
- Implement dashboard layout with prominent status hero section centered horizontally, positioned in upper third of viewport
- Design status card with generous padding (48px), large heading typography (H2), and prominent success icon
- Create multi-column grid below hero status displaying detailed health metrics: database connectivity, API response times, queue statistics, last update timestamp
- Add visual data representations: small line charts showing response time trends, status history timeline, or uptime percentage displays
- Include application header with branding, navigation, and connection status consistent with other pages
- Implement sidebar navigation matching Queues page structure to maintain cross-page consistency
- Use color-coded status badges with semantic meanings: green for healthy, yellow for degraded, red for critical
- Add actionable controls in top-right area: manual refresh button, auto-refresh toggle, settings access
- Design footer matching other pages with consistent positioning and content
- Implement responsive card grid showing individual component health statuses (PostgreSQL, PGMQ Extension, Admin API)
- Add timestamp with relative time display ("Last checked 5 seconds ago") with auto-updating behavior
- Consider historical status log table below primary status display showing recent health checks with timestamp and result columns
- Implement subtle animations on status transitions to draw attention to state changes

---

# Page – Queue Detail: Metrics Tab

## Screen size – 428×926
**Issues**
- Navigation drawer remains persistently visible on mobile, consuming approximately 25% of horizontal screen width and significantly reducing content area for metrics cards
- Metric cards exhibit excessive vertical padding (approximately 32px top/bottom), creating unnecessary scrolling when all five metrics could potentially fit in single viewport with optimized spacing
- Tab navigation shows "+1" overflow indicator without clear affordance, failing to communicate that additional tab (Metrics) is accessible through interaction
- Typographic hierarchy within cards lacks sufficient contrast: metric labels (Queue Length, Total Messages) appear only marginally larger than their descriptive text (Current messages, All-time)
- Large numeric values (7,011 and 2,061) display without thousands separators or formatted units, reducing readability and quick comprehension
- Secondary descriptive text (Current messages, All-time, Seconds) uses insufficient opacity differentiation from primary metric values, weakening information hierarchy
- Auto-refresh messaging appears as isolated paragraph text at bottom without visual container or status indicator styling, making it appear disconnected from metrics content
- Card borders and shadows create visual repetition without functional purpose, adding noise rather than improving content grouping
- Metric cards use uniform width regardless of content importance, treating critical metrics (Queue Length) equally to secondary metrics (Last Scrape)
- Page header section (Queue title, action buttons, tabs) consumes approximately 220px of vertical space, reducing viewport available for actual metrics content
- Last Scrape time format (19:37:49 UTC) displays precise timestamp without relative time context (e.g., "Updated 5 seconds ago"), reducing user-friendly temporal awareness
- No visual loading state or skeleton screens visible during metric refresh cycles, creating potential confusion during 30-second auto-refresh intervals

**Improvements**
- Implement collapsible navigation drawer with overlay pattern on mobile to reclaim horizontal space, or replace with bottom navigation bar for primary sections
- Reduce card vertical padding to 20px and optimize card spacing to 12px gaps to fit more metrics in initial viewport without scrolling
- Redesign tab overflow with horizontal scroll affordance or replace "+1" with visible "Metrics" tab label using condensed typography
- Strengthen typographic hierarchy: increase metric label font size to 16-18px and weight to 600 (semibold), reduce secondary text to 12-14px with 60% opacity
- Apply thousands separator formatting to large numbers (7,011 → 7,011 or 7.0k for more compact display) and add unit suffixes where appropriate
- Implement three-tier typographic scale: metric labels (largest, bold), numeric values (large, prominent), descriptive text (small, subdued)
- Redesign auto-refresh indicator as persistent status badge in header or floating notification strip with icon and last update timestamp
- Reduce card elevation to single subtle shadow or remove entirely, using only border treatment for cleaner visual presentation
- Consider two-column responsive grid for less critical metrics (Oldest/Newest Message Age side-by-side) to improve space utilization
- Consolidate page header by moving action buttons to navigation bar or sticky bottom sheet, reducing header height by approximately 80px
- Add relative time format alongside absolute timestamp: "Updated 5 seconds ago (19:37:49 UTC)" with progressive disclosure pattern
- Implement subtle pulse animation or progress indicator during active refresh cycles to provide feedback that metrics are updating
- Add visual emphasis to Queue Length as primary metric through larger card size, accent color border, or prominent placement in visual hierarchy
- Include trend indicators (↑ ↓ →) for metrics that change over time to provide at-a-glance status awareness without requiring mental comparison

## Screen size – 1160×720
**Issues**
- Metrics display in rigid single-column vertical stack utilizing only approximately 35% of available horizontal viewport width, leaving massive whitespace on right side of content area
- Card-based layout designed for mobile viewport fails to adapt to desktop space, creating awkward aspect ratio with tall narrow cards in wide horizontal space
- All five metric cards receive equal visual weight and sizing despite varying importance (Queue Length more critical than Last Scrape time)
- Navigation sidebar width (approximately 140px) creates suboptimal content area proportion, with metrics compressed to narrow column in expansive horizontal space
- Typographic scale appears undersized for desktop viewing distance, with metric labels sized for mobile reading distance without responsive scaling
- Tab navigation shows "+1" overflow indicator despite ample horizontal space available to display all three tabs (Messages, Archived, Metrics) inline
- Auto-refresh messaging positioned as trailing content without visual prominence, making critical real-time update information easy to overlook
- Metric value numbers (3, 6, 7,041, 2,091) lack formatted presentation with thousands separators or abbreviated notation, reducing professional appearance
- Secondary descriptive text (Current messages, All-time, Seconds, UTC) uses identical color treatment as labels, creating flat information hierarchy
- Footer anchors to viewport bottom rather than content bottom, creating disconnected floating appearance with significant empty space between content and footer
- No visual relationships or groupings between related metrics (Oldest/Newest Message Age could be paired, Queue Length/Total Messages could be related)
- Header action buttons (Send Message, Back to Queues) occupy prime real estate with unequal visual weight and inconsistent alignment pattern
- Missing contextual information or comparison data (e.g., queue health indicators, historical trends, threshold warnings) that would leverage additional desktop screen space

**Improvements**
- Implement responsive multi-column grid layout: arrange metrics in 2-3 column configuration to utilize horizontal space effectively and reduce scroll requirement
- Create visual hierarchy through card sizing: increase Queue Length and Total Messages card sizes to emphasize primary metrics, reduce Last Scrape to compact format
- Establish metric grouping with subtle visual containers: group Queue Length/Total Messages as "Capacity Metrics", Oldest/Newest Age as "Timing Metrics"
- Optimize navigation sidebar to collapsible state or reduce width to icon-only representation to maximize content area proportions
- Scale typography responsively for desktop: increase metric labels to 18-20px, numeric values to 32-40px for improved readability at desktop viewing distance
- Remove tab overflow indicator and display all tabs inline horizontally, utilizing available horizontal space for full navigation visibility
- Redesign auto-refresh indicator as prominent status banner or header badge with icon, timestamp, and optional manual refresh action for desktop context
- Apply number formatting: add thousands separators (7,041), consider abbreviated notation for large values (7.0k seconds), ensure consistent decimal precision
- Implement three-tier opacity hierarchy: metric labels (100%), numeric values (100%), descriptive text (60-70%) for clear visual separation
- Position footer to stick to content bottom or integrate refresh status into footer area to eliminate disconnected floating appearance
- Add visual connectors or grouping containers to related metrics using subtle background tints or border treatments to show relationships
- Consolidate header actions: align Send Message and Back to Queues on single horizontal line with consistent button styling and spacing
- Leverage additional space for contextual enhancements: add sparkline charts showing metric trends, color-coded health indicators (green/yellow/red), or comparison to average values
- Include data visualization options: allow toggle between card view and compact table view for users preferring higher information density on desktop
- Add hover states to metric cards with subtle background tint and optional tooltip showing historical values or trend direction
- Implement progressive disclosure: show primary metrics initially, offer "Show Advanced Metrics" expansion for additional queue health indicators to avoid overwhelming initial view

---

