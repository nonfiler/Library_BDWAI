using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Library.Data;
using Library.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Library.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BooksController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Book.Include(b => b.Author);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> Catalog()
        {
            var books = await _context.Book
                .Include(b => b.Author)
                .Include(b => b.Comments)
                .ToListAsync();

            return View(books);
        }

        public IActionResult ShowSearchForm()
        {
            return View();
        }

        public async Task<IActionResult> ShowSearchResults(string searchTerm)
        {
            searchTerm = searchTerm.ToLower();

            var searchResults = await _context.Book
                .Where(i => i.Title.ToLower().Contains(searchTerm) ||
                            i.Author.Name.ToLower().Contains(searchTerm) ||
                            i.Author.Surname.ToLower().Contains(searchTerm))
                .Include(b => b.Author)
                .ToListAsync();

            return View("Catalog", searchResults);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Author)
                .Include(b => b.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(b => b.ID == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddOrEditComment(int bookId, Comment comment)
        {
            var book = await _context.Book.Include(b => b.Comments).FirstOrDefaultAsync(b => b.ID == bookId);

            if (book == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Comment existingComment = null;
            if (comment.ID != 0)
            {
                existingComment = await _context.Comment.FirstOrDefaultAsync(c => c.ID == comment.ID && c.UserId == userId);
            }
            else
            {
                existingComment = await _context.Comment.FirstOrDefaultAsync(c => c.BookID == bookId && c.UserId == userId);
            }

            if (existingComment != null)
            {
                existingComment.Text = comment.Text;
                existingComment.Score = comment.Score;
            }
            else
            {
                var newComment = new Comment
                {
                    UserId = userId,
                    BookID = bookId,
                    Text = comment.Text,
                    Score = comment.Score
                };

                _context.Comment.Add(newComment);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = bookId });
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comment.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (comment == null || (comment.UserId != userId && !await _userManager.IsInRoleAsync(user, "Admin")))
            {
                return NotFound();
            }

            _context.Comment.Remove(comment);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["AuthorID"] = new SelectList(_context.Author, "ID", "Name");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBook([Bind("ID,Title,AuthorID,Genre,Description,Rating")] Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Catalog));
            }
            ViewData["AuthorID"] = new SelectList(_context.Set<Author>(), "ID", "ID", book.AuthorID);
            return View(book);
        }






        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["AuthorID"] = new SelectList(_context.Set<Author>(), "ID", "NameSurname", book.AuthorID);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,AuthorID,Genre,Description,Rating")] Book book)
        {
            if (id != book.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Catalog));
            }
            ViewData["AuthorID"] = new SelectList(_context.Set<Author>(), "ID", "NameSurname", book.AuthorID);
            return View(book);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Book.FindAsync(id);
            if (book != null)
            {
                _context.Book.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Catalog));
        }

        private bool BookExists(int id)
        {
            return _context.Book.Any(e => e.ID == id);
        }
    }
}
