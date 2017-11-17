create table Table1 (
	Table1Key int not null identity(1, 1),
	Table1Column1 nvarchar(255),
	Table1Column2 int not null,
	Table1Column3 date,
	Table1Column4 smalldatetime default(getdate()),
	Table1Column5 bit not null default(1),
	Table1Column6 nvarchar(1000),
	Table1Column7 int,
	Table1Column8 real,
	Table1Column9 nvarchar(255),
	constraint PK_Table1Key primary key (Table1Key)
)
go

create unique index IX_Table1Column1 on Table1 (Table1Column1)
go
create table Table2 (
	Table2Key int not null identity(1, 1),
	Table2Column1 int not null,
	Table2Column2 time,
	Table2Column3 int not null,
	Table2Column4 decimal(5,2) not null,
	Table2Column5 bit not null default(1),
	constraint PK_Table2Key primary key (Table2Key)
)
go

alter table Table2
add constraint FK_Table1_Table2 foreign key (Table2Column3)
references Table1 (Table1Column7)
on delete cascade
go