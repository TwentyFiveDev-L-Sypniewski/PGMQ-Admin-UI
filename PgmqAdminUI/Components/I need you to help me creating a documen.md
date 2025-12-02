I need you to help me creating a document that would help the AI Coding agent improving the UI/UX of the project by pointing out what is wrong with current design and how to fix it.
The goal is a markdown document written usign designer/UI/UX person's language that points out what is incorrect with current design and how it should be fixed.
It should be broken down per page and screen size.
I expect the plan to use vocabulary commonly understood by UI/UX/design people instead of snippets of code/css/html.
What are the crucial issues that should be addressed to make the UI/UX better?
- Improve visual hierarchy by using consistent font sizes and weights.
- Ensure adequate spacing and alignment for better readability.
- Use a cohesive color scheme that enhances usability and aesthetics.
- Responsive design: Ensure the layout adapts seamlessly across different screen sizes (mobile, tablet, desktop).

### Example of expected document format

```md
# Page - <page name>

## Screen size - 428 x 926

<What is incorrect with UI/UX>
<How to make it to look well and be functional>

### <Optionally - remarks regarding specific element on the page>

<What is incorrect with UI/UX>
<How to make it to look well and be functional>

## Screen size - 1160 x 720

<What is incorrect with UI/UX>
<How to make it to look well and be functional>

### <Optionally - remarks regarding specific element on the page>

<What is incorrect with UI/UX>
<How to make it to look well and be functional>

# Page - <another page name>
```

I want you to use Playwright to traverse the pages (main url: https://localhost:7201/) - the web server is running locally on my machine - and analyze the UI/UX of each page in both mobile and desktop viewports:
- Mobile: 428 x 926
- Desktop: 1160 x 720