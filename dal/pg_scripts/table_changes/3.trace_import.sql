ALTER TABLE CONTENT_ATTRIBUTE ADD COLUMN IF NOT EXISTS TRACE_IMPORT boolean NOT NULL DEFAULT false;
ALTER TABLE CONTENT ADD COLUMN IF NOT EXISTS TRACE_IMPORT_SCRIPT text NULL;
