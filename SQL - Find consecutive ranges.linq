<Query Kind="SQL">
  <Connection>
    <ID>ad592185-7d29-4bea-becd-0d0b5bffb6f4</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>localhost</Server>
    <DeferDatabasePopulation>true</DeferDatabasePopulation>
    <Database>LocalTelAppts</Database>
  </Connection>
</Query>

declare @t table
(
	id int
);

insert into @t(id)
values
	(1),
	(2),
	(3),
	(6),
	(7),
	(10),
	(12);



with cteRange as (
	select t.id as [from],
		(
			-- get the last slot of the consecutive slots 
			--(i.e., the least slot that does not have a next consecutive slot and is on or after the current slot)
			select isnull(min(innerT.id),t.id) from @t as innerT
			left outer join @t as innerNext on innerT.id+1 = innerNext.id
			where
				innerNext.id is null and
				innerT.id >= t.id
		) as [to]
	from @t as t
	-- join with next consecutive slot
	left outer join @t as [next] on t.id+1 = [next].id
	-- join with previous consecutive slot
	left outer join @t as [prev] on t.id-1 = [prev].id
	where 
		-- there is a next consecutive slot
		([next].id is not null and [prev].id is null)
		or
		-- there is neither a next nor previous slot (i.e., this is a single slot)
		([next].id is null and [prev].id is null)
)
select [from], [to], ([to]-[from]+1) as [slots]
from cteRange