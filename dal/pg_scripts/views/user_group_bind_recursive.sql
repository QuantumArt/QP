CREATE OR REPLACE VIEW user_group_bind_recursive
 AS
 WITH RECURSIVE r AS (
         SELECT user_group_bind.user_id,
            user_group_bind.group_id
           FROM user_group_bind
        UNION ALL
         SELECT ub.user_id,
            gg.parent_group_id
           FROM user_group_bind ub
             JOIN group_to_group gg ON ub.group_id = gg.child_group_id
        )
 SELECT r.user_id,
    r.group_id
   FROM r;
