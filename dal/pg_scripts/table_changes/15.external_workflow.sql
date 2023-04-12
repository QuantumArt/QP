CREATE SEQUENCE IF NOT EXISTS public.external_workflow_seq;

CREATE TABLE IF NOT EXISTS public.external_workflow (
    id numeric(18,0) NOT NULL DEFAULT nextval('external_workflow_seq'::regclass),
    created timestamptz NOT NULL,
    created_by varchar(100) NOT NULL,
    process_id varchar(36) NOT NULL,
    article_name varchar(255) NOT NULL,
    workflow_name varchar(255) NOT NULL,
    constraint pk_external_workflow primary key (id),
    constraint uq_external_workflow_process_id unique (process_id)
);

ALTER SEQUENCE external_workflow_seq
OWNED BY external_workflow.id;

CREATE SEQUENCE IF NOT EXISTS public.external_workflow_status_seq;

CREATE TABLE IF NOT EXISTS public.external_workflow_status (
    id numeric(18,0) NOT NULL DEFAULT nextval('external_workflow_status_seq'::regclass),
    external_workflow_id numeric(18, 0) NOT NULL,
    status varchar(255) NOT NULL,
    created timestamptz NOT NULL,
    created_by varchar(100) NOT NULL,
    constraint pk_external_workflow_status primary key (id),
    constraint fk_external_workflow_status_external_workflow_id foreign key (external_workflow_id) references public.external_workflow (id)
);

ALTER SEQUENCE external_workflow_status_seq
OWNED BY external_workflow_status.id;

CREATE SEQUENCE IF NOT EXISTS public.external_workflow_in_progress_seq;

CREATE TABLE IF NOT EXISTS public.external_workflow_in_progress (
    id numeric(18,0) NOT NULL DEFAULT nextval('external_workflow_in_progress_seq'::regclass),
    process_id numeric(18, 0) NOT NULL,
    current_status numeric(18, 0) NOT NULL,
    last_modified_by numeric(18,0) NOT NULL,
    article_id numeric(18,0) NOT NULL,
    workflow_id numeric(18,0) NOT NULL,
    constraint pk_external_workflow_in_progress primary key (id),
    constraint uq_external_workflow_in_progress_process_id unique (process_id),
    constraint fk_external_workflow_in_progress_process_id foreign key (process_id) references public.external_workflow (id),
    constraint fk_external_workflow_in_progress_current_status foreign key (current_status) references public.external_workflow_status (id),
    constraint fk_external_workflow_last_modified_by foreign key (last_modified_by) references public.users (user_id),
    constraint fk_external_workflow_article_id foreign key (article_id) references public.content_item(content_item_id),
    constraint fk_external_workflow_workflow_id foreign key (workflow_id) references public.content_item(content_item_id)
);

ALTER SEQUENCE external_workflow_in_progress_seq
OWNED BY external_workflow_in_progress.id;
