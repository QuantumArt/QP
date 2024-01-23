-- Table: content_data

-- DROP TABLE content_data;

CREATE TABLE IF NOT EXISTS content_item_ft
(
    content_item_id numeric(18,0) NOT NULL,
    ft_data tsvector,
    PRIMARY KEY (content_item_id),
    CONSTRAINT fk_content_item_ft_content_item FOREIGN KEY (content_item_id)
        REFERENCES content_item (content_item_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_content_item_ft_data
    ON content_item_ft USING gin
    (ft_data)
    TABLESPACE pg_default;





