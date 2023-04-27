ALTER TABLE IF EXISTS Schedules ADD COLUMN IF NOT EXISTS allow_concurrent_tasks boolean NOT NULL DEFAULT true;

ALTER TABLE IF EXISTS Schedules DROP COLUMN IF EXISTS allowconcurrenttasks;