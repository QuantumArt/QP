ALTER TABLE notifications_sent ADD COLUMN IF NOT EXISTS id int NOT NULL DEFAULT nextval('notifications_sent_seq'::regclass);

ALTER TABLE notifications_sent DROP CONSTRAINT IF EXISTS pk_notifications_sent;

ALTER TABLE notifications_sent ADD CONSTRAINT pk_notifications_sent PRIMARY KEY (id);
