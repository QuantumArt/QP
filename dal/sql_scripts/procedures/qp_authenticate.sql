ALTER procedure [dbo].[qp_authenticate](@login nvarchar(255), @password varchar(20), @use_nt_login bit = 0, @check_admin_access bit = 0)
as
begin
    declare @disabled bit, @hash binary(20), @salt binary(20), @user_id numeric, @auto_login bit
    if @use_nt_login = 1
    begin
        select @disabled = disabled, @user_id = user_id, @auto_login = AUTO_LOGIN from users where NT_LOGIN = @login
        if @user_id is null
        begin
            RAISERROR('Your Windows account is not mapped to any QP user', 16, 5)
            RETURN
        end
        else if @disabled = 1
        begin
            RAISERROR('Account is disabled. Contact <@MailForErrors@>', 16, 2)
            RETURN
        end
        else if @auto_login = 0
        begin
            RAISERROR('Auto login option is switched off for your account. Contact <@MailForErrors@>', 16, 6)
            RETURN
        end
    end
    else
    begin
        select @disabled = disabled, @hash = hash, @salt = salt, @user_id = user_id from users where login = @login
        if @user_id is null
        begin
            RAISERROR('Login doesn''t exist', 16, 1)
            RETURN
        end
        else if @disabled = 1
        begin
            RAISERROR('Account is disabled. Contact <@MailForErrors@>', 16, 2)
            RETURN
        end
        else if dbo.qp_get_hash(cast(@password as CHAR(20)), @salt) <> @hash
        begin
            RAISERROR('Password is incorrect', 16, 3)
            RETURN
        end
        else if @check_admin_access = 1
        begin
            if not exists(select * from USER_GROUP_BIND where GROUP_ID = 1 and USER_ID = @user_id)
            begin
                RAISERROR('Account is not a member of Administrators group', 16, 4)
                RETURN
            end
        end
    end

    update USERS set LAST_LOGIN = GETDATE() where USER_ID = @user_id
    select * from USERS where USER_ID = @user_id
    RETURN
end
go
