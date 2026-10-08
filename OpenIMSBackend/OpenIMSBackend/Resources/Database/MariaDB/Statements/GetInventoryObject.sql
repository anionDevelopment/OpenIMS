
select `Id`, `Type`, `Name`, `Description`, `ParentId`, `RealPlaceId`
	from InventoryObjects
	where `Id`=@Id;
