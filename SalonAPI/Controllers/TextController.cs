using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalonAPI.Models;
using SalonAPI.Models.DTOs;

namespace SalonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TextController : ControllerBase
    {

        private readonly DataContext context;

        public TextController(DataContext context)
        {
            this.context = context;

        }

        [HttpGet("GetAllText")]
        public async Task<ActionResult<List<TextDTO>>> GetAllText()
        {
            var allTextDTO = await context.Text.Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return Ok(allTextDTO);
        }

        [HttpGet("GetTextByKey")]
        public async Task<ActionResult<TextDTO>> GetTextByKey(string key)
        {
            var text = await context.Text.Where(x => x.Key.Equals(key))
                .Select(x => Mapper.MapToDTO(x))
                .FirstOrDefaultAsync();

            if (text == null) return NotFound("Text was not found");            

            return Ok(text);
        }


        [HttpPost("CreateText")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<TextDTO>>> CreateText(TextDTO textDTO)
        {
            var dbText = await context.Text.FirstOrDefaultAsync(x => x.Key == textDTO.Key);

            if (dbText != null) return BadRequest("There already exists a text string with this key");

            var newText = new Text()
            {
                Key = textDTO.Key,
                DanishValue = textDTO.DanishValue,
                EnglishValue = textDTO.EnglishValue
            };

            context.Text.Add(newText);
            await context.SaveChangesAsync();

            var allText = await context.Text.Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return Ok(allText);
        }

        [HttpPut("UpdateText")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<TextDTO>>> UpdateText(TextDTO textDTO)
        {
            var dbText = await context.Text.FirstOrDefaultAsync(x => x.Key == textDTO.Key);

            if (dbText == null) return NotFound("Text not found");

            dbText.DanishValue = textDTO.DanishValue;
            dbText.EnglishValue = textDTO.EnglishValue;

            await context.SaveChangesAsync();

            var allText = await context.Text.Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return Ok(allText);
        }

        [HttpDelete("DeleteText")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<TextDTO>>> DeleteText(string key)
        {
            var dbText = await context.Text.FirstOrDefaultAsync(x => x.Key == key);

            if (dbText == null) return NotFound("Text not found");

            context.Text.Remove(dbText);
            await context.SaveChangesAsync();

            var allText = await context.Text.Select(x => Mapper.MapToDTO(x)).ToListAsync();

            return Ok(allText);
        }


    }
}
