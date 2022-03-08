CREATE FULLTEXT CATALOG zapread_catalog AS DEFAULT;

--SELECT fulltextcatalogproperty('zapread_catalog', 'ItemCount'); 

CREATE FULLTEXT INDEX ON [dbo].[Comment] ([Text])
	KEY INDEX [PK_dbo.Comment]
	ON zapread_catalog 
	WITH STOPLIST = SYSTEM,
	CHANGE_TRACKING = AUTO;

CREATE FULLTEXT INDEX ON [dbo].[Post] ([Content])
	KEY INDEX [PK_dbo.Post]
	ON zapread_catalog 
	WITH STOPLIST = SYSTEM,
	CHANGE_TRACKING = AUTO;