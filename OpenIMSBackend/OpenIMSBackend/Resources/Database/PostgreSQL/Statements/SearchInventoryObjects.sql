
-- @SearchPattern is a like-pattern in which the wildcard-characters of the search-text are escaped with a backslash (the default escape-character of like).
select "Id", "Type", "Name", "Description", "ParentId", "RealPlaceId"
	from "InventoryObjects"
	where lower("Name") like lower(@SearchPattern)
		and ((@IncludeLocations and "Type"='Location')
			or (@IncludeBuildings and "Type"='Building')
			or (@IncludeAreas and "Type"='Area')
			or (@IncludeContainers and "Type"='Container')
			or (@IncludeItems and "Type"='Item'))
	order by "Name", "Id"
	limit @MaximalAmountOfResults;
