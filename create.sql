drop database if exists publishing_house_db;
create database if not exists publishing_house_db;
use publishing_house_db;

drop table if exists employee;
create table if not exists employee(
	email varchar(50) not null,
	name varchar(50),
	surnname varchar(50),
	salary float(15,5),
	hire_date datetime,
	username varchar(50) not null,
	password varbinary(50) not null,
	unique(username),
	primary key(email)
)engine = InnoDB;

drop table if exists publisher;
create table if not exists publisher(
	email varchar(50) not null,
	primary key(email),
	foreign key(email) references employee(email)
)engine = InnoDB;

drop table if exists editor;
create table if not exists editor(
	email varchar(50) not null,
	primary key(email),
	foreign key(email) references employee(email)
)engine = InnoDB;

drop table if exists paper;
create table if not exists paper(
	name varchar(50) not null,
	owner varchar(50) not null,
	publish_frequency enum('daily','weekly','monthly'),
	supervisor varchar(50) not null,
	primary key(name),
	foreign key(owner) references publisher(email),
	foreign key(supervisor) references editor(email)
)engine = InnoDB;

drop table if exists leaf;
create table if not exists leaf(
	paper_name varchar(50) not null,
	leaf_num int not null auto_increment,
	pages int default 30,
	publish_date datetime,
	published_by varchar(50),
	lf_not_sold int not null,
	lf_sold int not null,
	lf_printed int not null,
    unique(leaf_num),
	primary key(paper_name,leaf_num),
	foreign key(paper_name) references paper(name) on update cascade on delete cascade,
	foreign key(published_by) references employee(email) on update cascade on delete cascade
)engine = InnoDB;

drop table if exists works_at;
create table if not exists works_at(
	employee_email varchar(50) not null,
	paper_name varchar(50) not null,
	primary key(employee_email,paper_name),
	foreign key(employee_email) references employee(email) on update cascade on delete cascade,
	foreign key(paper_name) references paper(name) on update cascade on delete cascade
)engine = InnoDB;

drop table if exists journalist;
create table if not exists journalist(
	email varchar(50) not null,
	cv text,
	prior_exp date,
	job enum('chief editor','editor') not null,
	primary key(email),
	foreign key(email) references employee(email) on update cascade on delete cascade
)engine = InnoDB;

drop table if exists administrative;
create table if not exists administrative(
	email varchar(50) not null,
	duties enum('secretary','logistics'),
	street varchar(50),
	street_num int,
	city varchar(50),
	primary key(email),
	foreign key(email) references employee(email) on update cascade on delete cascade
)engine = InnoDB;

drop table if exists phone_numbers;
create table if not exists phone_numbers(
	admin_email varchar(50) not null,
	phone_number varchar(10),
	primary key(admin_email),
	foreign key(admin_email) references administrative(email) on update cascade on delete cascade
)engine = InnoDB;

drop table if exists category;
create table if not exists category(
	code int not null auto_increment,
	name varchar(50),
	description text,
	parent_cat int,
	primary key (code),
	foreign key(parent_cat) references category(code) on update cascade on delete cascade
)engine = InnoDB;

drop table if exists article;
create table if not exists article(
	article_path varchar(250) not null,
	title varchar(50),
	summary text,
	published_at int,
	belongs_to_cat int,
	article_num int not null,
	to_check enum('approved','rejected','to change'),
	approved_date datetime,
	pages_num int not null,
	start_page int not null,
	supl_path varchar(250),
	primary key(article_path),
	foreign key(published_at) references leaf(leaf_num) on update cascade on delete cascade,
	foreign key(belongs_to_cat) references category(code) on update cascade on delete cascade
)engine = InnoDB;

drop table if exists keywords;
create table if not exists keywords(
	keyword varchar(50) not null,
	article_path varchar(250) not null,
	primary key(keyword,article_path),
	foreign key(article_path) references article(article_path) on update cascade on delete cascade
)engine = InnoDB;

drop table if exists is_submitted_by;
create table if not exists is_submitted_by(
	journalist_email varchar(50) not null,
	article_path varchar(250) not null,
	submitted_date datetime,
	primary key(journalist_email,article_path),
	foreign key(journalist_email) references journalist(email) on update cascade on delete cascade,
	foreign key(article_path) references article(article_path) on update cascade on delete cascade
)engine = InnoDB;

insert into employee values('georgeOrwell@gmail.com','George','Orwell','1200','2000-03-25 12:30:55','George','Orwell'),
	('arthurClarke@gmail.com','Arthur','Clarke','1500','2001-05-14 14:35:54','ArthurC','Clarke'),
	('albertCamus@gmail.com','Albert','Camus','1000','2010-03-01 12:36:48','Albert','Camus'),
	('agathaCristie@hotmail.com','Agatha','Cristie','1100','1998-09-23 17:52:10','Agatha','Cristie'),
	('franzKafka@hotmail.com','Franz','Kafka','1000','2003-06-14 19:32:14','Franz','Kafka'),
	('simoneDeBeauvoir@yahoo.com','Simone','De Beauvoir','1300','1995-04-26 15:26:18','Simone','De Beauvoir'),
	('umbertoEco@gmail.com','Umberto','Eco','1000','1999-02-08 16:23:24','Umberto','Eco'),
	('erichFromm@gmail.com','Erich','Fromm','1500','1994-04-26 19:14:23','Erich','Fromm'),
	('arthurMiller@hotmail.com','Arthur','Miller','1000','2000-03-12 16:26:03','Arthur','Miller'),
	('gabrielGarciaMarquez@yahoo.com','Gabriel','Garcia Marquez','1200','2016-06-04 16:25:19','Gabriel','Garcia Marquez'),
	('williamGibson@gmail.com','William','Gibson','2000','2015-03-26 22:21:35','William','Gibson'),
	('oscarWilde@hotmail.com','Oscar','Wilde','2000','2014-12-24 17:26:59','Oscar','Wilde'),
	('jamesJoyce@yahoo.com','James','Joyce','2000','2013-11-04 14:25:34','James','Joyce'),
	('fyodorDostoyevsky@hotmail.com','Fyodor','Dostoyevsky','2000','2009-12-14 12:15:27','Fyodor','Dostoyevsky'),
	('virginiaWolf@gmail.com','Virginia','Wolf','2000','2008-09-29 12:26:55','Virginia','Wolf'),
	('charlesDickens@yahoo.com','Charles','Dickens','1500','2006-12-24 13:29:36','Charles','Dickens'),
	('williamShakespear@gmail.com','William','Shakespear','1600','2000-11-14 18:12:34','WilliamS','Shakespear'),
	('joanneKRowling@hotmail.com','Joanne','Rowling','1550','2016-12-14 19:25:42','Joanne','Rowling'),
	('johnRRTolkien@gmail.com','John','Tolkien','1300','2018-05-26 16:25:36','John','Tolkien'),
	('marcelProust@yahoo.com','Marcel','Proust','1200','2014-02-23 19:20:37','Marcel','Proust'),
	('antonChekhov@gmail.com','Anton','Chekhov','1000','2013-03-05 12:13:19','Anton','Chekhov');

insert into category values('0','Medicine','This is a description',null),
	('0','Jobs','This is a description',null),
	('0','Industry','This is a description',null),
	('0','Space','This is a description',null),
	('0','ISS','This is a description','4');

insert into publisher values('williamGibson@gmail.com'),
	('oscarWilde@hotmail.com'),
	('jamesJoyce@yahoo.com'),
	('fyodorDostoyevsky@hotmail.com'),
	('virginiaWolf@gmail.com');

insert into editor values('charlesDickens@yahoo.com'),
	('williamShakespear@gmail.com'),
	('joanneKRowling@hotmail.com'),
	('johnRRTolkien@gmail.com'),
	('marcelProust@yahoo.com');

insert into paper values('Le Monde','williamGibson@gmail.com','weekly','charlesDickens@yahoo.com'),
	('The Guardian','oscarWilde@hotmail.com','weekly','williamShakespear@gmail.com'),
	('Daily Mail','jamesJoyce@yahoo.com','daily','joanneKRowling@hotmail.com'),
	('BBC News','fyodorDostoyevsky@hotmail.com','daily','johnRRTolkien@gmail.com'),
	('Deutsche Welle','virginiaWolf@gmail.com','monthly','marcelProust@yahoo.com');

insert into works_at values('georgeOrwell@gmail.com','Le Monde'),
	('umbertoEco@gmail.com','Le Monde'),
	('williamGibson@gmail.com','Le Monde'),
	('charlesDickens@yahoo.com','Le Monde'),
	('arthurClarke@gmail.com','The Guardian'),
	('simoneDeBeauvoir@yahoo.com','The Guardian'),
	('oscarWilde@hotmail.com','The Guardian'),
	('williamShakespear@gmail.com','The Guardian'),
	('albertCamus@gmail.com','Daily Mail'),
	('erichFromm@gmail.com','Daily Mail'),
	('jamesJoyce@yahoo.com','Daily Mail'),
	('joanneKRowling@hotmail.com','Daily Mail'),
	('agathaCristie@hotmail.com','BBC News'),
	('arthurMiller@hotmail.com','BBC News'),
	('fyodorDostoyevsky@hotmail.com','BBC News'),
	('johnRRTolkien@gmail.com','BBC News'),
	('franzKafka@hotmail.com','Deutsche Welle'),
	('gabrielGarciaMarquez@yahoo.com','Deutsche Welle'),
	('virginiaWolf@gmail.com','Deutsche Welle'),
	('marcelProust@yahoo.com','Deutsche Welle'),
	('antonChekhov@gmail.com','Deutsche Welle');

insert into leaf values('Le Monde','0','10','2019-03-01 18:56:03','georgeOrwell@gmail.com',50,50,100),
	('The Guardian','0','5','2018-06-06 13:42:16','arthurClarke@gmail.com',60,40,100),
	('Daily Mail','0','2','2019-05-12 11:11:12','albertCamus@gmail.com',70,30,100),
	('BBC News','0','3','2019-07-25 12:25:59','agathaCristie@hotmail.com',80,20,100),
	('Deutsche Welle','0','4','2019-03-16 19:30:36','franzKafka@hotmail.com',90,10,100);

insert into article values('C:/Users/Articles/article1.doc','article1','article1 summary','1','1','1',null,null,'2','1',null),
	('C:/Users/Articles/article6.doc','article6','article6 summary','1','1','2',null,null,'2','3',null),
	('C:/Users/Articles/article2.doc','article2','article2 summary','2','2','1',null,null,'3','1',null),
	('C:/Users/Articles/article3.doc','article3','article3 summary','3','3','1',null,null,'1','1',null),
	('C:/Users/Articles/article4.doc','article4','article4 summary','4','4','1',null,null,'2','1',null),
	('C:/Users/Articles/article5.doc','article5','article5 summary','5','5','1',null,null,'5','1',null);

insert into keywords values('medicine','C:/Users/Articles/article1.doc'),
	('jobs','C:/Users/Articles/article2.doc'),
	('industry','C:/Users/Articles/article3.doc'),
	('space','C:/Users/Articles/article4.doc'),
	('ISS','C:/Users/Articles/article5.doc');

insert into journalist values('georgeOrwell@gmail.com','This is a CV','1995-06-26','chief editor'),
	('arthurClarke@gmail.com','This is a CV','2000-05-30','chief editor'),
	('albertCamus@gmail.com','This is a CV','2000-12-12','chief editor'),
	('agathaCristie@hotmail.com','This is a CV','1994-05-05','chief editor'),
	('franzKafka@hotmail.com','This is a CV','2000-03-24','chief editor'),
	('antonChekhov@gmail.com','This is a CV','2000-02-20','editor');


insert into administrative values('umbertoEco@gmail.com','secretary','6th Avenue','321','New York'),
	('simoneDeBeauvoir@yahoo.com','logistics','10th Avenue','125','New York'),
	('erichFromm@gmail.com','secretary','Rue National','23','Paris'),
	('arthurMiller@hotmail.com','logistics','Cripplegate','45','London'),
	('gabrielGarciaMarquez@yahoo.com','secretary','Foster Lane','234','London');

insert into phone_numbers values('umbertoEco@gmail.com','6972642123'),
	('simoneDeBeauvoir@yahoo.com','6984265125'),
	('erichFromm@gmail.com','6949265124'),
	('arthurMiller@hotmail.com','6972654189'),
	('gabrielGarciaMarquez@yahoo.com','6945623756');

insert into is_submitted_by values('georgeOrwell@gmail.com','C:/Users/Articles/article1.doc','2019-02-26 16:25:52'),
	('arthurClarke@gmail.com','C:/Users/Articles/article2.doc','2019-05-12 19:42:15'),
	('albertCamus@gmail.com','C:/Users/Articles/article3.doc','2019-04-11 16:32:23'),
	('agathaCristie@hotmail.com','C:/Users/Articles/article4.doc','2019-09-05 17:25:36'),
	('franzKafka@hotmail.com','C:/Users/Articles/article5.doc','2019-06-29 23:15:16'),
	('agathaCristie@hotmail.com','C:/Users/Articles/article6.doc','2019-07-05 20:25:45');

