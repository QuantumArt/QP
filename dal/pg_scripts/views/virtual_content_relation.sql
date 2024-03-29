CREATE OR REPLACE VIEW VIRTUAL_CONTENT_RELATION AS
	select  DISTINCT
			PA.CONTENT_ID AS BASE_CONTENT_ID,
			A.CONTENT_ID AS VIRTUAL_CONTENT_ID
			from CONTENT_ATTRIBUTE A
			JOIN CONTENT_ATTRIBUTE PA ON A.persistent_attr_id = PA.ATTRIBUTE_ID
	UNION ALL
	SELECT union_content_id AS BASE_CONTENT_ID,
		   virtual_content_id AS VIRTUAL_CONTENT_ID
	FROM union_contents
	UNION ALL
	SELECT real_content_id AS BASE_CONTENT_ID,
		   virtual_content_id AS VIRTUAL_CONTENT_ID
	FROM user_query_contents;


