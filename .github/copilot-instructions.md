# Copilot Instructions

## Project Guidelines
- Use Oqtane.Modules.Controls.RichTextEditor with @ref and GetHtml() to edit and persist HtmlContent fields instead of a plain textarea when implementing HtmlEditor behavior.
- Use migration version numbering pattern GIBS.Module.Entity.01.00.XX.00 (for example 01.00.02.00), not 01.00.00.XX. Increment the module/package version whenever a schema migration is added. Prefer conventional migration patterns and avoid raw SQL conditional migration scripts for schema changes.
- In GIBS.Module.Entity, templates are module-instance scoped.
- IsFeatured belongs only on the Entity model/table and should not exist on EntityField/GIBS_EntityField.