import styled from 'styled-components';

export const Container = styled.div`
  background: linear-gradient( to right, rgba(74, 164, 128), rgb(118, 209, 174));
  width: 100%;
  height: 84px;
  box-shadow: 0 6px 2px -2px gray;
`;

export const HeaderContainer = styled.div`
  display: flex;
  align-items: center;
  justify-content: left;
  padding: 24px;
  margin-left: 20px;
`;

export const Text = styled.p`
  font-family: Arial, Helvetica, sans-serif;
  font-weight: bold;
  text-transform: uppercase;
  font-size: 36px;
  color: #FFF;
  margin: 0 18px 0 10px;
`;