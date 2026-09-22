
insert into Settings(`Key`, `Value`)
	values(@Key, @Value)
	on duplicate key update `Value`=@Value;
