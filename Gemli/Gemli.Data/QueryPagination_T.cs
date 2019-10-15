using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    /// <summary>
    /// Describes how a query should paginate the result set with a strong type reference.
    /// </summary>
    public class QueryPagination<TModel> where TModel : DataModel
    {
        /// <summary>
        /// Constructs the pagination rule with default values of Page 1, unlimited items per page.
        /// </summary>
        public QueryPagination()
        {
            Page = 1;
            ItemsPerPage = int.MaxValue;
        }

        /// <summary>
        /// Indicates which page to be returned from the result set.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Indicates the number of items that should be in a page.
        /// </summary>
        public int ItemsPerPage { get; set; }

        /// <summary>
        /// Constructs the pagination rule with the specified values for the Page number and 
        /// items count per page.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="itemsPerPage"></param>
        public QueryPagination(int page, int itemsPerPage)
        {
            Page = page;
            ItemsPerPage = itemsPerPage;
        }

        /// <summary>
        /// This is a syntactical sugar constructor.
        /// Used for chaining, allows <see cref="OfItemsPerPage"/> to 
        /// return the query after the items count has been specified,
        /// while also setting the page number.
        /// </summary>
        /// <param name="query"></param>
        public QueryPagination(DataModelQuery<TModel> query)
        {
            _query = query;
        }

        /// <summary>
        /// Sets the <see cref="QueryPagination{TModel}.Page"/> property and returns
        /// this object again to allow the invocation of
        /// <see cref="OfItemsPerPage"/>. This is a syntactical sugar member.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public QueryPagination<TModel> this[int page]
        {
            get
            {
                Page = page;
                return this;
            }
        }

        /// <summary>
        /// Sets the number of items per page and returns the query
        /// that would be creating this object.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public DataModelQuery<TModel> OfItemsPerPage(int items)
        {
            ItemsPerPage = items;
            return _query;
        }

        private readonly DataModelQuery<TModel> _query;

    }
}
