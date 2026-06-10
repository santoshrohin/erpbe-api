CREATE OR ALTER PROCEDURE [dbo].[SP_EXCISE_TARIFF_MASTER_Select]


@E_CODE int,
@E_CM_COMP_ID int,
@E_TARIFF_NO varchar(50),
@E_COMMODITY varchar(50),
@E_BASIC float,
@E_SPECIAL float,
@E_EDU_CESS float,
@E_H_EDU float,
@E_TALLY_BASIC int,
@E_TALLY_SPECIAL int,
@E_TALLY_EDU int,
@E_TALLY_H_EDU int,

@ES_DELETE bit,
@MODIFY bit,
@Type varchar(50),
@E_TALLY_GST_EXCISE bit
AS

if @Type='CHECKUPDATE'
begin

Select 

E_CODE,
E_CM_COMP_ID, 
E_TARIFF_NO, 
E_COMMODITY, 
cast(E_BASIC as numeric(10,2)) as E_BASIC, 
cast(E_SPECIAL as numeric(10,2)) as E_SPECIAL, 
cast(E_EDU_CESS as numeric(10,2)) as E_EDU_CESS,
cast(E_H_EDU as numeric(10,2)) as E_H_EDU,
E_TALLY_BASIC,
E_TALLY_SPECIAL,
E_TALLY_EDU,
E_TALLY_H_EDU,

ES_DELETE,
MODIFY

from EXCISE_TARIFF_MASTER

where 
( @E_CODE is null or @E_CODE != E_CODE ) and
( @E_CM_COMP_ID is null or @E_CM_COMP_ID = E_CM_COMP_ID ) and
( @E_TARIFF_NO is null or @E_TARIFF_NO = E_TARIFF_NO ) and
--( @E_COMMODITY is null or @E_COMMODITY = E_COMMODITY ) and
--( @E_BASIC is null or @E_BASIC = E_BASIC ) and
--( @E_SPECIAL is null or @E_SPECIAL = E_SPECIAL ) and
--( @E_EDU_CESS is null or @E_EDU_CESS = E_EDU_CESS ) and
( @ES_DELETE is null or @ES_DELETE = ES_DELETE ) and
 @ES_DELETE = ES_DELETE and ( @E_TALLY_GST_EXCISE is null or @E_TALLY_GST_EXCISE = E_TALLY_GST_EXCISE )  ORDER BY E_COMMODITY

end
else if @type='FILLGRID'
begin
Select 

E_CODE,
E_CM_COMP_ID, 
E_TARIFF_NO, 
E_COMMODITY, 
cast(E_BASIC as numeric(10,2)) as E_BASIC, 
cast(E_SPECIAL as numeric(10,2)) as E_SPECIAL, 
cast(E_EDU_CESS as numeric(10,2)) as E_EDU_CESS,
cast(E_H_EDU as numeric(10,2)) as E_H_EDU,
E_TALLY_BASIC,
E_TALLY_SPECIAL,
E_TALLY_EDU,
E_TALLY_H_EDU,
ES_DELETE,
MODIFY

from EXCISE_TARIFF_MASTER

where 
 
( @E_CODE is null or @E_CODE = E_CODE ) and
( @E_CM_COMP_ID is null or @E_CM_COMP_ID = E_CM_COMP_ID) and
( @E_TARIFF_NO is null or @E_TARIFF_NO = E_TARIFF_NO ) and
--( @E_COMMODITY is null or @E_COMMODITY = E_COMMODITY ) and
--( @E_BASIC is null or @E_BASIC = E_BASIC ) and
--( @E_SPECIAL is null or @E_SPECIAL = E_SPECIAL ) and
--( @E_EDU_CESS is null or @E_EDU_CESS = E_EDU_CESS ) and
( @ES_DELETE is null or @ES_DELETE = ES_DELETE ) and
( @E_TALLY_GST_EXCISE is null or @E_TALLY_GST_EXCISE = E_TALLY_GST_EXCISE ) and
 @ES_DELETE = ES_DELETE ORDER BY E_COMMODITY

end
else
begin
Select 

E_CODE,
E_CM_COMP_ID, 
E_TARIFF_NO, 
E_COMMODITY, 
cast(E_BASIC as numeric(10,2)) as E_BASIC, 
cast(E_SPECIAL as numeric(10,2)) as E_SPECIAL, 
cast(E_EDU_CESS as numeric(10,2)) as E_EDU_CESS,
cast(E_H_EDU as numeric(10,2)) as E_H_EDU,
E_TALLY_BASIC,
E_TALLY_SPECIAL,
E_TALLY_EDU,
E_TALLY_H_EDU,
ES_DELETE,
MODIFY

from EXCISE_TARIFF_MASTER

where 
 
( @E_CODE is null or @E_CODE = E_CODE ) and
( @E_CM_COMP_ID is null or @E_CM_COMP_ID = E_CM_COMP_ID) and
( @E_TARIFF_NO is null or @E_TARIFF_NO = E_TARIFF_NO ) and
--( @E_COMMODITY is null or @E_COMMODITY = E_COMMODITY ) and
--( @E_BASIC is null or @E_BASIC = E_BASIC ) and
--( @E_SPECIAL is null or @E_SPECIAL = E_SPECIAL ) and
--( @E_EDU_CESS is null or @E_EDU_CESS = E_EDU_CESS ) and
( @ES_DELETE is null or @ES_DELETE = ES_DELETE ) and
 @ES_DELETE = ES_DELETE ORDER BY E_COMMODITY


end