ALTER TABLE CONTENT_ATTRIBUTE ADD COLUMN IF NOT EXISTS TRACE_IMPORT boolean NOT NULL DEFAULT false;
ALTER TABLE CONTENT ADD COLUMN IF NOT EXISTS TRACE_IMPORT_SCRIPT text NULL;
ALTER TABLE CONTENT_ATTRIBUTE ADD COLUMN IF NOT EXISTS DENY_PAST_DATES boolean NOT NULL DEFAULT false;
