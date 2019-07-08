-- Table: public.content_data

-- DROP TABLE public.content_data;

CREATE TABLE public.content_item_ft
(
    content_item_id numeric(18,0) NOT NULL,
    ft_data tsvector,
    PRIMARY KEY (content_item_id),
    CONSTRAINT fk_content_item_ft_content_item FOREIGN KEY (content_item_id)
        REFERENCES public.content_item (content_item_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

ALTER TABLE public.content_item_ft
    OWNER to postgres;


CREATE INDEX ix_content_item_ft_data
    ON public.content_item_ft USING gin
    (ft_data)
    TABLESPACE pg_default;





