update workflow set is_default = 1 where workflow_name = 'general' and not exists (select * from workflow where is_default = 1)
GO