ALTER TABLE site ADD COLUMN IF NOT EXISTS replace_urls_in_db boolean NOT NULL DEFAULT false;
ALTER TABLE site ADD COLUMN IF NOT EXISTS use_native_ef_types boolean NOT NULL DEFAULT false;