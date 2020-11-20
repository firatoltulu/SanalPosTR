import React from 'react';
import { Container, HeaderContainer, Text } from './styles';
import { FaDollarSign, FaCreditCard } from 'react-icons/fa';

export default function Header() {
  return (
    <Container>
      <HeaderContainer>
        <FaDollarSign color="#FFF" size={36} />
        <Text>Kredi kartı ile öde</Text>
        <FaCreditCard color="#FFF" size={36} />
      </HeaderContainer>
    </Container>
  );
};