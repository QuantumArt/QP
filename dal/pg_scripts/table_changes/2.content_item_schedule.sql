ALTER TABLE content_item_schedule
    ADD COLUMN IF NOT EXISTS start_date timestamp NULL;

ALTER TABLE content_item_schedule
    ADD COLUMN IF NOT EXISTS end_date timestamp NULL;

