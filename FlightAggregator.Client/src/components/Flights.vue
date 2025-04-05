<template>
    <div class="flights-container">
      <h1>Flights</h1>
      <div class="filters">
        <input v-model="filters.airline" placeholder="Airline" />
        <input v-model.number="filters.minPrice" type="number" placeholder="Min Price" />
        <input v-model.number="filters.maxPrice" type="number" placeholder="Max Price" />
        <button @click="fetchFlights">Search</button>
      </div>
      <div class="flights-list">
        <div v-for="flight in flights" :key="flight.Id" class="flight-card">
          <h2>{{ flight.Airline }}</h2>
          <p>Price: {{ flight.Price }} USD</p>
          <p>Available Seats: {{ flight.AvailableSeats }}</p>
          <p>Departure Date: {{ new Date(flight.DepartureDate).toLocaleDateString() }}</p>
          <input v-model.number="bookingSeats[flight.Id]" type="number" min="1" :max="flight.AvailableSeats" placeholder="Seats" />
          <button @click="bookFlight(flight)">Book</button>
        </div>
      </div>
    </div>
  </template>
  
  <script setup>
  import { ref, reactive, onMounted } from 'vue';
  import api from '../api'; 
  
  const flights = ref([]);
  const filters = reactive({
    airline: '',
    minPrice: null,
    maxPrice: null
  });
  const bookingSeats = reactive({}); 
  
  const fetchFlights = async () => {
    try {
      const params = Object.fromEntries(
        Object.entries(filters).filter(([_, v]) => v !== null && v !== '')
      );
  
      const response = await api.get('/aggregator/flights', { params });
      flights.value = response.data.Flights; 
    } catch (error) {
      console.error('Error fetching flights:', error);
    }
  };
  
  const bookFlight = async (flight) => {
    if (!bookingSeats[flight.Id] || bookingSeats[flight.Id] <= 0) {
      alert('Please enter a valid number of seats.');
      return;
    }
  
    const bookingRequest = {
      FlightId: flight.Id,
      Seats: bookingSeats[flight.Id],
      Source: flight.Source 
    };
  
    try {
      const response = await api.post('/aggregator/book-flight', bookingRequest);
      alert(`Booking successful: ${response.data}`);
    } catch (error) {
      console.error('Error booking flight:', error);
      alert('Failed to book flight. Please try again.');
    }
  };
  
  onMounted(fetchFlights);
  </script>
  
  <style scoped>
  .flights-container {
    max-width: 800px;
    margin: 0 auto;
    padding: 20px;
  }
  
  .filters {
    display: flex;
    gap: 10px;
    margin-bottom: 20px;
  }
  
  .flights-list {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 20px;
  }
  
  .flight-card {
    border: 1px solid #ccc;
    border-radius: 10px;
    padding: 15px;
    background-color: #f9f9f9;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    transition: transform 0.2s;
  }
  
  .flight-card:hover {
    transform: scale(1.02);
  }
  
  .book-button {
    display: inline-block;
    margin-top: 10px;
    padding: 10px 15px;
    background-color: #007bff;
    color: white;
    text-align: center;
    border-radius: 5px;
    text-decoration: none;
  }
  
  .book-button:hover {
    background-color: #0056b3;
  }
  </style>