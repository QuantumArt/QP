if not exists (select * From ACTION_TYPE where code = 'auto_resize')
insert into [ACTION_TYPE] (NAME, CODE, REQUIRED_PERMISSION_LEVEL_ID, ITEMS_AFFECTED)
	  values ('Auto resize',  'auto_resize', 2,1)
