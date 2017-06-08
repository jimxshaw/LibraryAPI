using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private ILibraryRepository _libraryRepository;

        public AuthorsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet()]
        public IActionResult GetAuthors()
        {
            // Retrieve authors entity.
            var authorsFromRepo = _libraryRepository.GetAuthors();

            // Use automapper to map entity to DTO.
            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);

            // We always return Ok because even when there are no authors,
            // the underlying authors entity still exist.... it'd be just an
            // empty collection. No need to return 404.
            return Ok(authors);
        }

        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);

            if (authorFromRepo == null)
            {
                return NotFound();
            }

            var author = Mapper.Map<AuthorDto>(authorFromRepo);

            return Ok(author);
        }

        [HttpPost]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = Mapper.Map<Author>(author);

            // Entity has been added to the db context but not actually saved.
            _libraryRepository.AddAuthor(authorEntity);
            // The Save method must be called to enforce saving to the db.
            // It returns a boolean. The save succeeded, true or false.
            if (!_libraryRepository.Save())
            {
                // By throwing an Exception instead of returning a 500, we'll 
                // force our exception handling middle in Startup to take over
                // if things go wrong.
                throw new Exception("Creating an author failed on saved.");
                //return StatusCode(500, "A problem happened with handling your request.");
            }

            // Map result after the author has been saved to the db.
            // The new author entity will contain the newest id.
            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);

            // We return a response with a location header that contains the URI
            // where the newly created author can be found. 
            // The action called is GetAuthor, which needs an id for its route. 
            // The actual author that'll be serialized to the response body is the returned author dto.
            return CreatedAtRoute("GetAuthor", new { id = authorToReturn.Id }, authorToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (_libraryRepository.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }
    }
}
