SELECT COUNT(*)
FROM `BlogTags` AS `b`
WHERE NOT (`b`.`IsDeleted`) AND (`b`.`Name` COLLATE utf8mb4_unicode_ci LIKE @_8__locals1_pattern ESCAPE '\')