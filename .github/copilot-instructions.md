# Copilot Instructions

## Project Guidelines
- Use Oqtane.Modules.Controls.RichTextEditor with @ref and GetHtml() to edit and persist HtmlContent fields instead of a plain textarea when implementing HtmlEditor behavior.
- Use migration version numbering pattern GIBS.Module.Entity.01.00.XX.00 (for example 01.00.02.00), not 01.00.00.XX. Increment the module/package version whenever a schema migration is added.
- In GIBS.Module.Entity, templates are module-instance scoped.