using Microsoft.AspNetCore.Mvc;
using ReviewsRatings.Models;
using ReviewsRatings.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewsRatings.Controllers
{
    [Route("reviews-and-ratings-demo/api/")]
    public class RoutesController : ControllerBase
    {
        private readonly IProductReviewService _productReviewsService;

        public RoutesController(IProductReviewService productReviewsService)
        {
            _productReviewsService = productReviewsService ?? throw new ArgumentNullException(nameof(productReviewsService));
        }

        [HttpPost]
        [Route("reviews")]
        public async Task<IActionResult> PostReviewsAsync(ICollection<Review> reviews)
        {
            var ids = new List<int>();
            foreach(Review review in reviews)
            {
                var reviewsResponse = await _productReviewsService.NewReview(review);
                ids.Add(reviewsResponse.Id);
            }

            return Json(ids);
        }

        [HttpPost]
        [Route("review/{id}")]
        public async Task<IActionResult> PostReviewAsync(int id, Review review)
        {
            bool hasShopperReviewed = await _productReviewsService.HasShopperReviewed(VtexIdentity.Name, review.ProductId);
            if (hasShopperReviewed)
            {
                return Json("Duplicate Review");
            }

            bool hasShopperPurchased = await _productReviewsService.ShopperHasPurchasedProduct(VtexIdentity.Name, review.ProductId);

            var reviewResponse = await _productReviewsService.NewReview(new Review
            {
                ProductId = review.ProductId,
                Rating = review.Rating,
                ShopperId = VtexIdentity.Name,
                Title = review.Title,
                Text = review.Text,
                VerifiedPurchaser = hasShopperPurchased
            });
            return Json(reviewResponse.Id);
        }

        [HttpDelete]
        [Route("reviews")]
        public async Task<IActionResult> DeleteReviewsAsync(ICollection<int> reviewIds)
            => Json(await _productReviewsService.DeleteReviewAsync(reviewIds.ToArray()));

        [HttpDelete]
        [Route("review/{id}")]
        public async Task<IActionResult> DeleteReviewAsync(int id)
            => Json(await _productReviewsService.DeleteReviewAsync(new[] { id }));

        [HttpPatch]
        [Route("reviews")]
        public IActionResult ModifyReviews()
            => BadRequest();

        [HttpPatch]
        [Route("review/{id}")]
        public async Task<IActionResult> ModifyReviewAsync(int id, Review review)
            => Json(await _productReviewsService.EditReview(review));

        [HttpGet]
        [Route("review/{id}")]
        public async Task<IActionResult> GetReviewAsync(int id)
            => Json(await _productReviewsService.GetReview(id));

        [HttpGet]
        [Route("review/{id}")]
        public async Task<IActionResult> GetReviewsAsync(int id, [FromQuery(Name = "product_id")] string? productId, int? from, int? to, 
            [FromQuery(Name = "order_by")] string orderBy,
            [FromQuery(Name = "search_term")] string search,
            [FromQuery(Name = "status")] string status)
        {
            var searchResult = !string.IsNullOrEmpty(productId) ?
                await _productReviewsService.GetReviewsByProductId(productId)
                    : await _productReviewsService.GetReviews();

            var searchData = _productReviewsService.FilterReviews(searchResult, search, orderBy, status);
            searchData = _productReviewsService.LimitReviews(searchData, from ?? 0, to ?? 3);

            SearchResponse searchResponse = new SearchResponse
            {
                Data = new DataElement { data = searchData },
                Range = new SearchRange { From = from ?? 0, To = to ?? 3, Total = searchData.Count }
            };
                    
            return Json(searchResponse);
        }

        [HttpGet]
        [Route("rating/{productId}")]
        public async Task<IActionResult> GetReviewRatingAsync(string productId)
        {
            var average = await _productReviewsService.GetAverageRatingByProductId(productId);
            var searchResult = await _productReviewsService.GetReviewsByProductId(productId);
                    
            return Json(new RatingResponse
            {
                Average = average,
                TotalCount = searchResult.Count
            });
        }
    }
}
