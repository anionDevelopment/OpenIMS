
select "Id"
	from "InventoryObjects"
	where "RealPlaceId"=@Id
	limit 1;
