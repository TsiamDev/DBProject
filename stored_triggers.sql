delimiter $
drop procedure if exists leaf_info$
create procedure leaf_info(in leaf_num int,in paper_name varchar(50))
	begin
		declare sum_pages int;
		declare leaf_pages int;

		declare title varchar(50);
		declare name varchar(50);
		declare surnname varchar(50);
		declare approved_date datetime;
		declare start_page int;
		declare pages_num int;
		declare flag int;
		declare cur cursor for
			select article.title, employee.name,employee.surnname,article.approved_date,article.start_page,article.pages_num from article 
			left join is_submitted_by on is_submitted_by.article_path=article.article_path 
			left join journalist on journalist.email=is_submitted_by.journalist_email 
			left join employee on employee.email=journalist.email where published_at=leaf_num;
		declare continue handler for not found set flag=1;


		set flag = 0;

		#available pages
		select sum(article.pages_num) into sum_pages from article where published_at=leaf_num group by published_at;
		select pages into leaf_pages from leaf where leaf.leaf_num=leaf_num;
		if(leaf_pages - sum_pages > 0) then
			select 'there are',leaf_pages-sum_pages,'more free pages in the leaf';
		end if;

		open cur;
			fetch cur into title,name,surnname,approved_date,start_page,pages_num;
			while (flag = 0) do 
				select title,name as editor_name,surnname as editor_surnname,approved_date,start_page,pages_num; 
				fetch cur into title,name,surnname,approved_date,start_page,pages_num;
			end while;
		close cur;
	end$

#call leaf_info(1,'sdf')$

drop procedure if exists calc_jour_salary$
create procedure calc_jour_salary(in jour_email varchar(50))
	begin
		declare h_date datetime;
		declare p_exp date;
		declare diff int;
		declare raise float(10,3);
		declare sal float(15,5);

		select employee.hire_date,journalist.prior_exp,employee.salary into h_date,p_exp,sal from employee 
		left join journalist on journalist.email=employee.email where journalist.email=jour_email;

		#select h_date,p_exp;
		select datediff(h_date,p_exp) into diff;
		set diff=diff/30;
		set raise = (diff*0.5)/100;
		#select sal;
		set sal = sal + sal*raise;
		#select sal;
		#select employee.salary from employee where employee.email=jour_email;
		update employee set employee.salary=sal where employee.email=jour_email;
		#select employee.salary from employee where employee.email=jour_email;
	end$

#call calc_jour_salary('georgeOrwell@gmail.com')$

drop trigger if exists insert_employee$
create trigger insert_employee before insert on employee for each row
	begin
		set new.salary = 650;
	end$

drop trigger if exists check_jour$
create trigger check_jour after insert on is_submitted_by for each row
	begin
		declare j enum('chief editor','editor');
		#declare a_path varchar(250);
		declare j_email varchar(50);
		declare art_path varchar(250);
		set j_email = new.journalist_email;
		set art_path = new.article_path;

		select journalist.job into j from journalist where journalist.email=j_email;
		if(j = 'chief editor') then
			#select article.article_path into a_path from is_submitted_by 
			#left join article on article.article_path=is_submitted_by.article_path where is_submitted_by.journalist_email=new.journalist_email;

			update article set article.to_check='approved',article.approved_date=now() where article.article_path=art_path;
		end if;
	end$

#insert into is_submitted_by values('georgeOrwell@gmail.com','C:/Users/Articles/article1.doc','2019-02-26 16:25:52')$

drop trigger if exists check_cap$
create trigger check_cap before insert on article for each row
	begin
		declare sum_pages int;
		declare ps_num int;
		declare flag int;
		declare cur cursor for
			select article.pages_num from article where article.published_at = new.published_at;
		declare continue handler for not found set flag=1;
		set flag=0;
		set sum_pages=0;

		open cur;
			fetch cur into ps_num;
			while (flag = 0) do 
				set sum_pages = sum_pages + ps_num;
				fetch cur into ps_num;
			end while;
		close cur;
		select leaf.pages into ps_num from leaf where leaf.leaf_num = new.published_at;
		if(ps_num - sum_pages <= 0) then
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Not enough pages left in the leaf';
		end if;
	end$

#insert into article values('C:/Users/Articles/article7.doc','article7','article7 summary','1','1','1',null,null,'21','1');

drop procedure if exists check_user_pwd$
create procedure check_user_pwd(in username varchar(50),in password varchar(50),out result varchar(50),out found_e varchar(50))
	begin
		declare flag int;
		declare pwd varbinary(50);
		declare email varchar(50);
		select employee.email into email from employee where employee.username=username;
		#select email;
		set flag = -25;
		set found_e = 'not found';
		set result = 'not found';

		if(email is not null) then
			#person exists, check the password
			select employee.password into pwd from employee where employee.username=username;
			set flag = strcmp(pwd,password);
			#select flag,pwd,password;
			if(flag = 0) then
				#password matches, find the type of employee
				select journalist.email into found_e from journalist where journalist.email=email;
				if(found_e not like 'not found') then 
					set result = 'journalist';
				end if;
				if(found_e like 'not found') then
					select administrative.email into found_e from administrative where administrative.email=email;
					if(found_e not like 'not found') then 
						set result = 'administrative';
					end if;
				end if;
				if(found_e like 'not found') then 
					select publisher.email into found_e from publisher where publisher.email=email;
					if(found_e not like 'not found') then 
						set result = 'publisher';
					end if;
				end if;
				if(found_e like 'not found') then
					select editor.email into found_e from editor where editor.email=email;
					if(found_e not like 'not found') then 
						set result = 'editor';
					end if;
				end if;
				#select found_e,result;
			end if;
		end if;
	end$

#call check_user_pwd('Charles','Dickens',@res,@found)$
#select @res$

drop procedure if exists get_article$
create procedure get_article(in email varchar(50))
	begin
		declare pp_name varchar(50);
		select paper.name into pp_name from paper 
		left join works_at on works_at.paper_name=paper.name where works_at.employee_email=email;

		select article.article_path,article.title,article.to_check,article.start_page from leaf 
		left join article on leaf.leaf_num=article.published_at where leaf.paper_name=pp_name;
	end$

#call get_article('georgeOrwell@gmail.com')$

drop procedure if exists get_cat_code$
create procedure get_cat_code()
	begin
		select category.code from category;
	end$

#call get_cat_code()$

drop procedure if exists get_sal_info$
create procedure get_sal_info(in months int)
	begin
		declare email varchar(50);
		declare sal float(15,5);
		declare total_sal float(15,5);
		declare flag int;
		declare cur cursor for
			select employee.salary,employee.email from employee;
		declare continue handler for not found set flag=1;

		set flag = 0;
		set total_sal = 0;
		open cur;
			fetch cur into sal,email;
			while (flag = 0) do 
				set total_sal = total_sal + sal;
				fetch cur into sal,email;
			end while;
		close cur;
		set total_sal = total_sal * months;
		select total_sal as 'total salary';

		set flag = 0;
		open cur;
			fetch cur into sal,email;
			while (flag = 0) do 
				select email, sal * months;
				fetch cur into sal,email;
			end while;
		close cur;

	end$

#call get_sal_info(2)$

drop procedure if exists get_leaf_info$
create procedure get_leaf_info()
	begin
		select * from leaf;
	end$

drop procedure if exists get_paper_info$
create procedure get_paper_info(in owner varchar(50))
	begin
		select * from paper where paper.owner=owner;
	end$

drop procedure if exists get_leaf_printed$
create procedure get_leaf_printed()
	begin
		select leaf.leaf_num,leaf.lf_printed from leaf;
	end$

drop procedure if exists get_leaf_sales$
create procedure get_leaf_sales()
	begin
		declare num int;
		declare printed int;
		declare not_sold int;
		declare flag int;
		declare cur cursor for
			select leaf.leaf_num,leaf.lf_printed,leaf.lf_not_sold from leaf;
		declare continue handler for not found set flag=1;

		set flag = 0;
		open cur;
			fetch cur into num,printed,not_sold;
			while (flag = 0) do 
				select num as leaf_num,printed-not_sold as 'sold_leafs';
				fetch cur into num,printed,not_sold;
			end while;
		close cur;
	end$

drop procedure if exists get_submitted_article$
create procedure get_submitted_article(in email varchar(50))
	begin
		select * from article left join is_submitted_by on is_submitted_by.article_path=article.article_path 
			where is_submitted_by.journalist_email=email;
	end$

#call get_submitted_article('georgeOrwell@gmail.com')$

drop procedure if exists add_editor$
create procedure add_editor(in email varchar(50),in old_editor varchar(50),in papern varchar(50))
	begin
		declare found int;
		declare em varchar(50);
		declare flag int;
		declare cur cursor for
			select editor.email from editor;
		declare continue handler for not found set flag=1;

		set found = 0;
		set flag = 0;
		open cur;
			fetch cur into em;
			while (flag = 0) do 
				if(em like email) then
					set found = 1;
				end if;
				fetch cur into em;
			end while;
		close cur;
		if(found = 0) then
			#editor not found add new editor and delte old one
			insert into editor values(email);
			update paper set paper.supervisor=email where paper.name=papern;
			delete from editor where editor.email=old_editor;
		end if;
	end$