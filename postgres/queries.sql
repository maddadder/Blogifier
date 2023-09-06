-- query all posts and authors 
SELECT a.*, p.*
FROM public."Posts" p
JOIN public."Authors" a
ON p."AuthorId" = a."Id";

-- Get authors grouped by post count
SELECT a."Id", a."Email", COUNT(p."Id") AS "PostCount"
FROM public."Posts" p
JOIN public."Authors" a
ON p."AuthorId" = a."Id"
GROUP BY a."Id", a."Email"
ORDER BY "PostCount" DESC;
