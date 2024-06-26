using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            this._context = context;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                    .Include(u => u.Sender)
                    .Include(u => u.Recipient)
                    .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
                 .Include(x => x.Connections)
                 .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(
            MessageParams messageParams)
        {
            var query = _context.Messages
                    .OrderByDescending(m => m.MessageSent)
                    .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                    .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u =>
                        u.RecipientUsername.ToLower() == messageParams.Username.ToLower() &&
                        u.RecipientDeleted == false),
                "Outbox" => query.Where(u =>
                        u.SenderUsername.ToLower() == messageParams.Username.ToLower() &&
                        u.SenderDeleted == false),
                _ => query.Where(u =>
                        u.RecipientUsername.ToLower() == messageParams.Username.ToLower() &&
                        u.RecipientDeleted == false &&
                        u.DateRead == null)
            };

            return await PagedList<MessageDto>.CreateAsync(query,
                    messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsename,
                    string recipientUsername)
        {
            var messages = await _context.Messages
                    .Where(m =>
                            (m.Recipient.UserName == currentUsename &&
                               m.RecipientDeleted == false &&
                               m.Sender.UserName == recipientUsername) ||
                               (m.Recipient.UserName == recipientUsername &&
                               m.SenderDeleted == false &&
                               m.Sender.UserName == currentUsename)
                            )
                    .OrderBy(m => m.MessageSent)
                    .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();
            
            /* NOTA: Usando ProjectTo<>(), ya no se ocupa el 
             .Include(u => u.Sender).ThenInclude(p => p.Photos)
             .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                            El ProjectTo se encarga de todo!!!! */



            var unreadMessages = messages.Where(m => m.DateRead == null &&
                       m.RecipientUsername == currentUsename).ToList();
            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
                // No actualizo BD, hasta en el evento OnConnectedAsync en MessageHub
            }
            return (messages);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
                .Include(c => c.Connections)
                .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }
    }
}