using Games.Data;
using Games.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameAPIController : ControllerBase


    {
        private readonly ILogger<GameAPIController> _logger;

        public GameAPIController(ILogger<GameAPIController> logger)
        {
            _logger = logger;
        }

        [HttpGet]  //tümünün listelendiği kısım
        [Route("All", Name = "GetAllGames")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<GameDTO>> GetGames()
        {
            _logger.LogInformation("Getting All Games");
            return Ok(GameStore.gameList);

        }


        [HttpGet("{id:int}", Name ="GetGames")]   // id'ye göre listelenen kısım
      
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<GameDTO> GetGames(int id)
        {

            //400 Client Error
            if (id <= 0)
            {
                _logger.LogError("Get Game Error With ID" + id);
                return BadRequest("Lütfen 0'dan büyük bir sayı giriniz!"); 
            }

            var games = GameStore.gameList.FirstOrDefault(x => x.ID == id);


            //404 Not Found
            if (games == null)
            {
                return NotFound($"{id} Numaralı ID'ye Sahip Oyun Bulunamadı."); 
            }

            return Ok(games); //200


        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<GameDTO> CreateGame([FromBody] GameDTO newGameDTO)


        {

            if (GameStore.gameList.FirstOrDefault(x=>x.Name.ToLower() == newGameDTO.Name.ToLower()) != null) //aynı isme sahip oyun varsa mesaj gönder
            {
                ModelState.AddModelError("CustomError", "Bu Oyun Zaten Mevcut!");
                return BadRequest(ModelState);
            }


            if (newGameDTO == null)
            {
                return BadRequest(); //400
            }

            if (newGameDTO.ID > 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            newGameDTO.ID = GameStore.gameList.OrderByDescending(x=>x.ID).FirstOrDefault().ID+1;  //en son kaç numaralı ID varsa +1 ekleyip eklemeye devam et 

            GameStore.gameList.Add(newGameDTO);

            return CreatedAtRoute("GetGames",new {id= newGameDTO.ID}, newGameDTO);
        }

        [HttpDelete("{id:int}", Name = "DeleteGame")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteGame(int id)
        {
            if (id <= 0)
            {
                return BadRequest(); //400
            }

            var game = GameStore.gameList.FirstOrDefault(x=>x.ID == id);

            if (game == null)
            {
                return NotFound(); //404
            }

            GameStore.gameList.Remove(game);

            return NoContent(); //204 sildiğimizde geriye bir şey dönmez o yüzden NoContent kullandık
        }


        [HttpPut("{id:int}", Name = "UpdateGame")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdatedGame(int id, [FromBody] GameDTO updateGame)
        {
            if (updateGame == null || id != updateGame.ID) //null veya veri tabanındaki ID'ye eşit değilse
            {
                return BadRequest();
            }

            var game = GameStore.gameList.FirstOrDefault(x => x.ID == id);
            game.Name = updateGame.Name;
            game.Publisher = updateGame.Publisher;
            game.Description = updateGame.Description;
            game.ReleaseDate = updateGame.ReleaseDate;
            game.Price = updateGame.Price;

            return NoContent();

        }
          //PATCH KULLANIMI
          // {
          //  "op": "replace",    --> burada yapmak istediğimiz operasyonu yazarız (add,remove,replace,move,copy,test)
          //  "path": "/name",   --> güncellenecek adresin yolu
          //  "value": "Barry"  --> güncellenecek değer
          //},

        [HttpPatch("{id:int}", Name = "UpdatePartialGame")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdatePartialGame(int id,JsonPatchDocument<GameDTO> patchDTO)
        {
            if (patchDTO == null || id ==0)
            {
                return BadRequest(); //400
            }

            var game = GameStore.gameList.FirstOrDefault(x => x.ID == id);
            if (game == null)
            {
                return BadRequest();
            }

            patchDTO.ApplyTo(game,ModelState); //json dosyasını game'e uygulamasını istiyoruz eğer hata olursa modelstate ile tutulsun

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }

    };

}
