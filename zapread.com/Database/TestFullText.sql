--SELECT [dbo].Comment.CommentId 
--FROM [dbo].Comment
--WHERE FREETEXT ([dbo].[Comment].[Text], 'is a test')  
--GO

-- This query inspects the catalog
--SELECT display_term, column_id, document_count 
--FROM sys.dm_fts_index_keywords (DB_ID('zapread_dev'), OBJECT_ID('Comment'))

SELECT i.rank, Text, a.CommentId 
    FROM freetexttable(Comment,Text,'work') as i
    inner join Comment a
    on i.[key] = a.[CommentId]
    order by i.rank desc

SELECT i.rank as rank, Text, a.CommentId, a.TimeStamp, a.Score, a.TotalEarned, a.IsDeleted, a.IsReply, a.TimeStampEdited
    FROM freetexttable(Comment, Text, 'work') as i
    inner join Comment a
    on i.[key] = a.[CommentId]
    order by i.rank desc