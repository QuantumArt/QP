ALTER TABLE backend_action_log ADD COLUMN IF NOT EXISTS user_ip varchar(50);

CREATE TABLE IF NOT EXISTS backend_action_log_user_groups
(
    id int not null default nextval('backend_action_log_user_group_seq'::regclass)
        constraint pk_backend_action_log_user_groups primary key,
    backend_action_log_id int4 not null constraint fk_backend_action_log_user_groups_backend_action_log_id references backend_action_log(id) on delete cascade,
    group_id numeric(18,0) not null constraint  fk_backend_action_log_user_groups_group_id references user_group(group_id)
)
