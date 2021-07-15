update workflow set is_default = true where workflow_name = 'general' and not exists (select * from workflow where is_default);
