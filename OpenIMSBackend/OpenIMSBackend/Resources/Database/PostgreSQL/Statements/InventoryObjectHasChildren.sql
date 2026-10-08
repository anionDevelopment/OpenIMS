
select "Id"
	from "InventoryObjects"
	where "ParentId"=@Id
	limit 1;
