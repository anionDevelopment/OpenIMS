
insert into "Settings"("Key", "Value")
	values(@Key, @Value)
	on conflict("Key") do update set "Value"=@Value;
