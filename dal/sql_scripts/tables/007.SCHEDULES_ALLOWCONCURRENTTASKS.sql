IF	EXISTS (SELECT NULL FROM sys.[tables] WHERE name = 'Schedules') AND
	NOT EXISTS (SELECT NULL FROM sys.[columns] WHERE NAME = 'AllowConcurrentTasks' AND [object_id] = OBJECT_ID('Schedules'))
ALTER TABLE dbo.Schedules ADD AllowConcurrentTasks bit NOT NULL DEFAULT 1;