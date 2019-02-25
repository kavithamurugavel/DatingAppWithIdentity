using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/users/{userID}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _datingRepo;
        private readonly IMapper _mapper;
        public MessagesController(IDatingRepository datingRepo, IMapper mapper)
        {
            _mapper = mapper;
            _datingRepo = datingRepo;
        }

        [HttpGet("{id}", Name="GetMessage")]
        public async Task<IActionResult> GetMessage(int userID, int id)
        {
            if(userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _datingRepo.GetMessage(id);
            if(messageFromRepo == null)
                return NotFound();

            return Ok(messageFromRepo);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userID, [FromQuery]MessageParams messageParams)
        {
            if(userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageParams.UserID = userID;

            var messagesFromRepo = await _datingRepo.GetMessagesForUser(messageParams);

            var messages = _mapper.Map<IEnumerable<MessageToReturnDTO>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize, 
                                messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);

            return Ok(messages);
        }

        [HttpGet("thread/{recipientID}")]
        public async Task<IActionResult> GetMessageThread(int userID, int recipientID)
        {
            if(userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messagesFromRepo = await _datingRepo.GetMessageThread(userID, recipientID);

            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDTO>>(messagesFromRepo);

            return Ok(messageThread);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userID, MessageForCreationDTO messageForCreationDTO)
        {
            var sender = await _datingRepo.GetUser(userID, false);
            
            if(sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageForCreationDTO.SenderID = userID;
            var recipient = await _datingRepo.GetUser(messageForCreationDTO.RecipientID, false);
            
            if(recipient == null)
                return BadRequest("Could not find user");

            var message = _mapper.Map<Message>(messageForCreationDTO);

            _datingRepo.Add(message);
            
            if(await _datingRepo.SaveAll())
            {
                // mapping it back so that we show only the DTO's information and not the 
                // entire message/user information (i.e. with all the variables).
                // Also automapper automatically maps sender and receiver info in the MessageToReturnDTO
                var messageToReturn = _mapper.Map<MessageToReturnDTO>(message);
                return CreatedAtRoute("GetMessage", new {id = message.ID}, messageToReturn);
            }
            throw new Exception("Creating the message failed on save");
        }

        // the id here is the message id
        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userID)
        {
            if(userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _datingRepo.GetMessage(id);

            if(messageFromRepo.SenderID == userID)
                messageFromRepo.SenderDeleted = true;

            if(messageFromRepo.RecipientID == userID)
                messageFromRepo.RecipientDeleted = true;

            // deleting only when both sides of conversation are deleting
            if(messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
                _datingRepo.Delete(messageFromRepo);

            if(await _datingRepo.SaveAll())
                return NoContent();

            throw new Exception("Error deleting the message");
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageRead(int userID, int id)
        {
            if(userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var message = await _datingRepo.GetMessage(id);

            if(message.RecipientID != userID)
                return Unauthorized();

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await _datingRepo.SaveAll();

            return NoContent();
        }
    }
}