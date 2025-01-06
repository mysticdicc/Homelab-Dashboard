using dankapi.Data;
using danklibrary;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace dankapi.Controllers
{
    [ApiController]
    public class DashboardController : Controller
    {
        [HttpGet]
        [Route("[controller]/get/all")]
        public string GetAll()
        {
            using var context = new danknetContext();

            return JsonConvert.SerializeObject(context.DashboardItems.ToList());
        }

        [HttpGet]
        [Route("[controller]/get/byid")]
        public string GetById(int ID)
        {
            using var context = new danknetContext();
            return JsonConvert.SerializeObject(context.DashboardItems.Where(x => x.ID == ID).ToList());
        }

        [HttpGet]
        [Route("[controller]/get/byname")]
        public string GetByName(string name)
        {
            using var context = new danknetContext();
            return JsonConvert.SerializeObject(context.DashboardItems.Where(x => x.DisplayName.Contains(name)).ToList());
        }

        [HttpPost]
        [Route("[controller]/post/new")]
        public async Task<Results<BadRequest<string>, Created<DashboardItem>>> New(DashboardItem item)
        {
            using var context = new danknetContext();

            bool validObject;

            if (DashboardItem.IsValid(item))
            {
                validObject = true;
            }
            else
            {
                validObject = false;
            }

            if (validObject)
            {
                try
                {
                    context.DashboardItems.Add(item);
                    await context.SaveChangesAsync();

                    return TypedResults.Created(item.ID.ToString(), item);
                }
                catch (Exception ex)
                {
                    return TypedResults.BadRequest(ex.Message);
                }
            }
            else
            {
                return TypedResults.BadRequest("Object invalid.");
            }
        }

        [HttpPut]
        [Route("[controller]/put/update")]
        public async Task<Results<BadRequest<string>, Ok<DashboardItem>>> Update(DashboardItem item)
        {
            using var context = new danknetContext();

            //check object is valid
            bool validObject;

            if (DashboardItem.IsValid(item))
            {
                validObject = true;
            }
            else
            {
                validObject = false;
            }

            bool validID;
            DashboardItem? updateItem;

            if (context.DashboardItems.Where(x => x.ID == item.ID).ToList().Count == 1)
            {
                validID = true;
                updateItem = context.DashboardItems.Find(item.ID);
            }
            else
            {
                validID = false;
                updateItem = null;
            }

            if (validObject && validID)
            {
                if (null != updateItem)
                {
                    //update object
                    if (updateItem.DisplayName != item.DisplayName)
                    {
                        updateItem.DisplayName = item.DisplayName;
                    }

                    if (updateItem.Icon != item.Icon)
                    {
                        updateItem.Icon = item.Icon;
                    }

                    if (updateItem.URL != item.URL)
                    {
                        updateItem.URL = item.URL;
                    }

                    if (updateItem.Description != item.Description)
                    {
                        updateItem.Description = item.Description;
                    }

                    if (updateItem.Icon != item.Icon) 
                    { 
                        updateItem.Icon = item.Icon;
                    }

                    //submit to db
                    try
                    {
                        context.DashboardItems.Update(updateItem);
                        await context.SaveChangesAsync();

                        return TypedResults.Ok(updateItem);
                    }
                    catch (Exception ex)
                    {
                        return TypedResults.BadRequest($"{ex.Message}");
                    }
                }
                else
                {
                    return TypedResults.BadRequest("Unable to fetch object from db");
                }
            }
            else if (validID && (validObject == false))
            {
                return TypedResults.BadRequest("Object invalid");
            }
            else if (validObject && (validID == false))
            {
                return TypedResults.BadRequest("ID doesn't exist in DB");
            }
            else if ((validObject == false) && (validID == false))
            {
                return TypedResults.BadRequest("Object invalid and ID doesn't exist in DB");
            }
            else
            {
                return TypedResults.BadRequest("Unknown error");
            }
        }

        [HttpDelete]
        [Route("[controller]/delete/byitem")]
        public async Task<Results<BadRequest<string>, Ok<DashboardItem>>> Delete(DashboardItem item)
        {
            using var context = new danknetContext();

            if (DashboardItem.IsValid(item))
            {
                if (context.DashboardItems.Where(x => x.ID == item.ID).ToList().Count == 1)
                {
                    DashboardItem? deleteItem = context.DashboardItems.Find(item.ID);
                    
                    if (null != deleteItem)
                    {
                        try
                        {
                            context.DashboardItems.Remove(deleteItem);
                            await context.SaveChangesAsync();

                            return TypedResults.Ok(deleteItem);
                        } 
                        catch (Exception ex)
                        {
                            return TypedResults.BadRequest($"{ex.Message}");
                        }
                    } 
                    else
                    {
                        return TypedResults.BadRequest("Unable to track db object");
                    }
                } 
                else
                {
                    return TypedResults.BadRequest("Invalid number of objects returned from db");
                }
            } 
            else
            {
                return TypedResults.BadRequest("Invalid object");
            }
        }
    }
}
