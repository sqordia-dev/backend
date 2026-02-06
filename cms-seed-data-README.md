# CMS Seed Data File

The file `cms-seed-data.sql` contains the complete CMS seed data with proper conflict handling.

## Structure

1. **BEGIN** transaction
2. **CmsVersion INSERT** with `ON CONFLICT ("Id")` handling
3. **CmsContentBlocks INSERT** statements, each with `ON CONFLICT ("CmsVersionId", "BlockKey", "Language")` handling
4. **COMMIT** transaction

## To Complete the File

The current file includes:
- ✅ CmsVersion insert with conflict handling
- ✅ Dashboard labels (EN & FR) with conflict handling
- ⚠️ Missing: All other sections from:
  - `cms-seed-all-pages.sql` (Dashboard empty states, tips, Profile sections)
  - `cms-seed-missing-blocks.sql` (Additional dashboard and profile blocks)
  - `cms-seed-remaining-pages.sql` (Auth, Questionnaire, Create Plan, Subscription, Onboarding, Legal, Global Navigation)

## Pattern for Adding Sections

Each INSERT statement should follow this pattern:

```sql
INSERT INTO "CmsContentBlocks" (
    "Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", 
    "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted"
)
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'block.key', 0, 'Content', 'en', 1, 'section.key', NULL, NOW(), NOW(), false),
-- ... more values ...
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET
    "BlockType" = EXCLUDED."BlockType",
    "Content" = EXCLUDED."Content",
    "SortOrder" = EXCLUDED."SortOrder",
    "SectionKey" = EXCLUDED."SectionKey",
    "Metadata" = EXCLUDED."Metadata",
    "LastModified" = NOW(),
    "IsDeleted" = false;
```

## Foreign Key Constraints

- ✅ CmsVersion must be inserted first (no FK dependencies)
- ✅ CmsContentBlocks references CmsVersion via CmsVersionId (FK constraint)
- ✅ All inserts respect the unique constraint: (CmsVersionId, BlockKey, Language)

## Running the Seed File

```bash
psql -U your_user -d your_database -f cms-seed-data.sql
```

The file is idempotent - it can be run multiple times safely due to conflict handling.
