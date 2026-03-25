# Campaign Builder Guide

## Overview
The Campaign Builder is an advanced tree-based UI for creating and managing test campaign execution flows with full control over test groups, test cases, and individual function definitions.

## How to Use Campaign Builder

### 1. Create a Campaign
1. Navigate to **Campaigns** → **Create Campaign**
2. Fill in:
   - **Campaign Name** (required)
   - **Project** (required - select from dropdown)
   - **Description** (optional)
   - **Setup** (optional - select test setup to use for execution)
   - **Tags** (optional - comma separated)
3. Click **Create Campaign**
4. You'll be redirected to the Campaign Builder

### 2. Campaign Builder Interface

The builder has **3 panels**:

#### LEFT PANEL - Test Case Library
- Shows all available test cases
- **Search** test cases using the search box
- **Drag** test cases into groups to add them

#### CENTER PANEL - Tree Builder
- **Campaign Header**: Shows campaign name, project, and status
- **Tree Structure**: 
  - Campaign (root)
    - Test Groups (blue gradient cards)
      - Test Cases (yellow gradient cards)
        - Functions (orange gradient cards)

#### RIGHT PANEL - Properties
- Shows detailed properties when you select a group, test case, or function
- Edit settings, parameters, and overrides here

### 3. Building Your Campaign

#### Add Test Groups
1. Click **Add Group** button in toolbar
2. A new test group appears with default name "Test Group 1"
3. Click on the group to edit properties in the right panel:
   - Group Name
   - Description
   - Execution Order
   - Retry Count
   - Delay Between Retries (seconds)
   - Abort on Failure (checkbox)
   - Enable/Disable Group

#### Add Test Cases to Groups
1. **Drag** a test case from the left library panel
2. **Drop** it into a test group
3. The test case is added with all its functions auto-populated
4. Test case shows:
   - Order number badge
   - Enable/disable status indicator (green/gray dot)
   - Test case name
   - Number of functions
   - Inline retry and delay controls

#### Expand/Collapse
- Click the **chevron icon** (▼) next to groups or test cases to expand/collapse
- Groups show their test cases when expanded
- Test cases show their functions when expanded

#### View and Edit Functions
1. Expand a test case to see all its functions
2. Each function shows:
   - Order number
   - Function name
   - Execution type (REST/SCPI/SDK/SSH)
   - Operation type (READ/WRITE/FILE_UPLOAD/FILE_DOWNLOAD)
   - Enable/disable toggle
3. Click the **sliders icon** on a function to edit its parameters in the right panel

### 4. Function Management

#### Add New Functions
1. Expand a test case
2. Click **Add Function** button at the bottom of the functions list
3. A modal popup shows all available function definitions
4. **Search** for functions using the search box
5. **Click** on a function to add it to the test case

#### Reorder Functions
- **Drag** functions using the grip handle (⋮⋮)
- **Drop** them in the desired order
- Order numbers update automatically

#### Edit Function Parameters
1. Click the **sliders icon** on any function
2. Right panel shows:
   - Function definition details (read-only)
   - Execution and operation types
   - **Parameter Overrides table**
3. Enter custom values for each parameter
4. These values override the default values during campaign execution

#### Remove Functions
- Click the **X (remove) icon** on any function to delete it from the campaign
- This only removes it from the campaign, not from the function library

### 5. Inline Controls

#### On Test Groups:
- **Retry**: Number of times to retry the entire group if it fails
- **Delay**: Seconds to wait between retries
- **Toggle**: Enable/disable the entire group
- **Gear icon**: View/edit group properties
- **Delete icon**: Remove the group from campaign

#### On Test Cases:
- **Retry count** (small badge with ↻ icon)
- **Delay** (small badge with clock icon)
- **Toggle**: Enable/disable the test case
- **Sliders icon**: View/edit test case settings
- **X icon**: Remove test case from group

#### On Functions:
- **Toggle**: Enable/disable individual function
- **Sliders icon**: Edit function parameters
- **X icon**: Remove function from test case

### 6. Save Campaign
1. After building your campaign structure
2. Click **Save Campaign** button in the toolbar
3. All changes are saved to MongoDB
4. Toast notification confirms successful save

### 7. Run Campaign (Coming Soon)
- Click **Run Campaign** button to execute the entire campaign
- Execution will follow the structure:
  1. Groups execute in order
  2. Within each group, test cases execute in order
  3. Within each test case, functions execute in order
  4. Retry and delay logic applied at each level
  5. Abort on failure stops execution if configured

## Visual Indicators

### Status Dots
- 🟢 **Green**: Enabled (will execute)
- ⚫ **Gray**: Disabled (will be skipped)

### Color Scheme
- **Blue Gradient**: Test Groups
- **Yellow Gradient**: Test Cases  
- **Orange Gradient**: Functions
- **Purple Connectors**: Visual flow lines

### Order Badges
- **Numbered circles** show execution order
- Test cases: Large numbered badge
- Functions: Smaller numbered badge

## Tips & Best Practices

1. **Name your groups logically**: e.g., "Setup Phase", "Main Tests", "Cleanup"
2. **Use order numbers**: Control exact execution sequence
3. **Set retry counts wisely**: Balance between reliability and execution time
4. **Parameter overrides**: Use campaign-specific values without modifying function definitions
5. **Enable/Disable**: Temporarily skip groups/test cases/functions without removing them
6. **Save frequently**: Click Save Campaign after major changes
7. **Drag for efficiency**: Drag test cases from library instead of searching
8. **Expand only what you need**: Keep the tree collapsed for better overview

## Keyboard Shortcuts
- **Ctrl+F**: Focus search in test case library (when available)
- **Esc**: Close function selector modal

## Troubleshooting

### Test case doesn't show functions after dragging
- Make sure the test case has steps, actions, and operations defined
- Check browser console for errors
- Refresh the page and try again

### Changes not saving
- Check that you clicked "Save Campaign" button
- Verify MongoDB connection is working
- Check browser console for API errors

### Can't drag test cases
- Ensure test cases exist in the library (left panel)
- Check that group is expanded to accept drops
- Try refreshing the page

## Next Steps
After building your campaign:
1. **Execute** the campaign (feature coming soon)
2. **View results** and execution logs
3. **Compare** runs to track improvements
4. **Clone** successful campaigns for similar projects
5. **Share** campaigns with your team

---

For more information, see:
- [README.md](README.md) - Main project documentation
- [FEATURES_SUMMARY.md](FEATURES_SUMMARY.md) - Complete feature list
- API documentation at `/swagger` (when available)
