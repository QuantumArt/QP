exec qp_drop_existing 'qp_GetPermittedItemsAsQuery', 'IsProcedure'
go

CREATE PROCEDURE [dbo].[qp_GetPermittedItemsAsQuery]
(
  @user_id numeric(18,0)=0,
  @group_id numeric(18,0)=0,
  @start_level int=2,
  @end_level int=4,
  @entity_name varchar(100)='content_item',
  @parent_entity_name varchar(100)='',
  @parent_entity_id numeric(18,0)=0,
  @SQLOut varchar(8000) OUTPUT
)
AS

SET NOCOUNT ON

Declare @sPermissionTable varchar(200)
Declare @sHide varchar(50)
Declare @NewLine char(2)
Declare @sUnion varchar(20)
Declare @sSelectUser varchar(200)
Declare @sSelectGroup varchar(8000)
Declare @sSQL varchar(8000)
Declare @srGroupInList varchar (30)
Declare @srLevelIncrement varchar (30)
Declare @sTemp varchar(8000)
Declare @sWhereParentEntity varchar (8000)
Declare @sDefaultSQL varchar (8000)
Declare @sGroupBy varchar (200)
Declare @intIncrement int
Declare @CurrentLevelAddition int
Declare @sSQLStart varchar(300)
Declare @sSQLEnd varchar (600)

/***********************************/
/**** Declare Table Variables   ****/
/***********************************/
declare @ChildGroups table
(
	group_id numeric(18,0) PRIMARY KEY
)

declare @ParentGroups table
(
	group_id numeric(18,0) PRIMARY KEY
)

declare @UsedGroups table
(
	group_id numeric(18,0)
)

declare @TempParentGroups table
(
	group_id numeric(18,0) PRIMARY KEY
)
/***********************************/

select @NewLine = CHAR(13) + CHAR(10)
Select @intIncrement = 10
Select @CurrentLevelAddition = 0
Select @sSQLStart = ' select ' + @entity_name + '_id, cast(min(pl) as int)%10 as permission_level, max(hide) as hide from ('
Select @sSQLEnd = ') as qp_zzz group by qp_zzz.' + @entity_name + '_id HAVING cast(min(pl) as int)%10 >= ' + Cast(@start_level AS varchar) + ' AND cast(min(pl) as int)%10 <= ' + Cast(@end_level AS varchar)

Select @sGroupBy =  ' group by ' + @entity_name + '_id '
Select @sWhereParentEntity = ''
select @sPermissionTable = @entity_name + '_access_PermLevel'

if @parent_entity_name != '' and @parent_entity_id != 0
Begin
   Select @sPermissionTable = @sPermissionTable + '_' + @parent_entity_name
   Select @sWhereParentEntity = ' and ' + @parent_entity_name+ '_id=' + Cast(@parent_entity_id As varchar) + ' '
End

if @entity_name = 'content'
	set @sHide = ', MAX(CONVERT(int, hide)) as hide'
else
	set @sHide = ', 0 as hide'

select @sSQL = ''
select @sTemp = null
Select @srGroupInList = '<@_group_in_list_@>'
Select @srLevelIncrement = '<@_increment_level_@>'
select @sUnion = @NewLine + ' Union All ' + @NewLine
select @sSelectUser = ' select ' + @entity_name + '_id, max(permission_level) as pl' + @sHide + ' from ' + @sPermissionTable +  ' with(nolock) where user_id=' + Cast(@user_id AS varchar) + @NewLine
                      + @sWhereParentEntity + @NewLine
select @sSelectGroup = ' select ' + @entity_name + '_id, max(permission_level) + ' + @srLevelIncrement + ' as pl' + @sHide + ' from ' + @sPermissionTable +  ' with(nolock) where group_id in (' + @srGroupInList + ')' + @NewLine
                      + @sWhereParentEntity + @NewLine
select @sDefaultSQL = ' select 0 as ' + @entity_name + '_id, 0 as permission_level' + @sHide + ' from ' + @sPermissionTable


if @user_id > 0
Begin
   Select @sSQL = @sSelectUser + @sGroupBy
   insert into @ChildGroups (group_id) select distinct group_id from user_group_bind where user_id = @user_id
   Select @CurrentLevelAddition = @CurrentLevelAddition + @intIncrement
End

if @group_id > 0 AND @user_id <= 0
Begin
   insert into @ChildGroups(group_id) values (@group_id)
End

if (select count(*) from @ChildGroups) = 0
Begin
   if @sSQL != '' Select @SQLOut = @sSQL
   else Select @SQLOut = @sDefaultSQL
   return
End

SELECT @sTemp = COALESCE(@sTemp + ', ', '') + CAST(group_id AS varchar) FROM @ChildGroups
if @sSQL != '' Select @sSQL = @sSQL + @sUnion
Select @sSQL = @sSQL + Replace( Replace(@sSelectGroup,@srLevelIncrement,@CurrentLevelAddition), @srGroupInList, @sTemp )
Select @sSQL = @sSQL + @sGroupBy

insert into @UsedGroups(group_id) select group_id from @ChildGroups

WHILE 1=1
BEGIN
    Select @CurrentLevelAddition = @CurrentLevelAddition + @intIncrement
    select @sTemp = null
	insert into @ParentGroups (group_id) select distinct gtg.parent_group_id from group_to_group gtg inner join @ChildGroups cg on gtg.child_group_id = cg.group_id
    if (select count(*) from @ParentGroups) = 0 BREAK

    /* need to check that parent groups are not appearing in child groups */
    insert into @TempParentGroups (group_id) select pg.group_id from @ParentGroups pg where pg.group_id not in(select cg.group_id from @ChildGroups cg) and pg.group_id not in (select group_id from @UsedGroups)
    if (select count(*) from @TempParentGroups) != 0
    Begin
		SELECT @sTemp = COALESCE(@sTemp + ', ', '') + CAST(group_id AS varchar) FROM @TempParentGroups
		if @sSQL != '' Select @sSQL = @sSQL + @sUnion
		Select @sSQL = @sSQL + Replace( Replace(@sSelectGroup,@srLevelIncrement,@CurrentLevelAddition), @srGroupInList, @sTemp )
		Select @sSQL = @sSQL + @sGroupBy
        insert into @UsedGroups (group_id) select group_id from @TempParentGroups
    End

    delete @ChildGroups
    delete @TempParentGroups
    insert into @ChildGroups (group_id) select (group_id) from @ParentGroups
    delete @ParentGroups
    CONTINUE
END

Select @SQLOut = @sSQLStart + @sSQL + @sSQLEnd
return
