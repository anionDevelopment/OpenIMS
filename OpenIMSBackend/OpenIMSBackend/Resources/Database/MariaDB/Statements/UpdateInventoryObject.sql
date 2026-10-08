
update InventoryObjects
	set `Name`=@Name, `Description`=@Description, `ParentId`=@ParentId, `RealPlaceId`=@RealPlaceId
	where `Id`=@Id;
